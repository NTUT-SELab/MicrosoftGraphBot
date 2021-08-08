using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using MicrosoftGraphAPIBot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    public class DefaultGraphApi
    {
        private readonly BotDbContext db;
        private readonly ILogger logger;
        private readonly IHttpClientFactory clientFactory;

        private readonly static List<string> scopes = new List<string> {
            "offline_access user.read",
            OutlookApi.Scope,
            OneDriveApi.Scope,
            PersonalContactsApi.Scope
        };
        public readonly static string Scope = string.Join(' ', scopes);

        /// <summary>
        /// Create a new DefaultGraphApi instance.
        /// </summary>
        /// <param name="botDbContext"></param>
        /// <param name="clientFactory"></param>
        public DefaultGraphApi(BotDbContext botDbContext, ILogger<DefaultGraphApi> logger, IHttpClientFactory clientFactory) => 
            (this.db, this.logger, this.clientFactory) = (botDbContext, logger, clientFactory);

        /// <summary>
        /// 取得 Graph service client
        /// </summary>
        /// <param name="token"> access token </param>
        /// <returns> The object of GraphServiceClient. </returns>
        public static GraphServiceClient GetGraphServiceClient(string token)
        {
            IAuthenticationProvider authProvider = new DelegateAuthenticationProvider(async (requestMessage) =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                _ = await Task.FromResult<object>(null);
            });

            GraphServiceClient graphClient = new GraphServiceClient(authProvider);
            return graphClient;
        }

        /// <summary>
        /// 驗證 Azure 應用程式是否有效
        ///
        /// https://docs.microsoft.com/en-us/azure/media-services/previous/media-services-rest-connect-with-aad#get-the-access-token-using-postman
        /// </summary>
        /// <param name="email"> 應用程式持有者的 email </param>
        /// <param name="clientId"> Application (client) ID </param>
        /// <param name="clientSecret"> Client secrets </param>
        /// <returns> True 為有效的 Azure 應用程式，False 為無效的 Azure 應用程式 </returns>
        public async Task<bool> IsValidApplicationAsync(string email, string clientId, string clientSecret)
        {
            logger.LogInformation($"Verify azure application: email: {email}, clientId: {clientId}");
            Dictionary<string, string> body = new Dictionary<string, string>()
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "resource", "https://rest.media.azure.net" }
            };

            var formData = new FormUrlEncodedContent(body);
            string tenant = GetTenant(email);
            Uri url = new Uri($"https://login.microsoftonline.com/{tenant}/oauth2/token");
            HttpClient httpClient = clientFactory.CreateClient();
            var buffer = await httpClient.PostAsync(url, formData);

            string json = await buffer.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(json);
            if (jObject.Property("access_token") != null)
            {
                logger.LogInformation("Verify azure application: Success");
                return true;
            }

            logger.LogError(json);
            return false;
        }

        /// <summary>
        /// Get o365 user token.
        /// </summary>
        /// <param name="clientId"> Application (client) ID </param>
        /// <param name="code"> The authorization_code that the app requested. 
        /// The app can use the authorization code to request an access token for the target resource. 
        /// Authorization_codes are very short lived, typically they expire after about 10 minutes. </param>
        /// <returns> (access token, refresh token) </returns>
        public async Task<(string, string)> GetTokenAsync(Guid clientId, string code)
        {
            logger.LogInformation($"Get Token: clientId: {clientId}, code: {code}");
            AzureApp azureApp = await db.AzureApps.FindAsync(clientId);

            if (azureApp == null)
                throw new BotException("Azure 應用程式不存在");

            Dictionary<string, string> body = new Dictionary<string, string>()
            {
                { "client_id", clientId.ToString() },
                { "scope", Scope },
                { "code", code },
                { "redirect_uri", BindHandler.AppUrl },
                { "grant_type", "authorization_code" },
                { "client_secret", azureApp.Secrets }
            };

            var formData = new FormUrlEncodedContent(body);
            string tenant = GetTenant(azureApp.Email);
            Uri url = new Uri($"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token");
            HttpClient httpClient = clientFactory.CreateClient();
            var buffer = await httpClient.PostAsync(url, formData);

            string json = await buffer.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(json);
            if (jObject.Property("access_token") != null)
            {
                logger.LogInformation("Get Token: Success");
                return (jObject["access_token"].ToString(), jObject["refresh_token"].ToString());
            }

            logger.LogError(json);
            throw new BotException("獲取 Token 失敗");
        }

        /// <summary>
        /// Reflash o365 user token.
        /// </summary>
        /// <param name="appAuth"> O365 App auth object </param>
        /// <returns> (access token, 別名) </returns>
        public async Task<(string, string)> ReflashTokenAsync(AppAuth appAuth)
        {
            Dictionary<string, string> body = new Dictionary<string, string>()
            {
                { "client_id", appAuth.AzureApp.Id.ToString() },
                { "scope", Scope },
                { "refresh_token", appAuth.RefreshToken },
                { "redirect_uri", BindHandler.AppUrl },
                { "grant_type", "refresh_token" },
                { "client_secret", appAuth.AzureApp.Secrets }
            };

            var formData = new FormUrlEncodedContent(body);
            string tenant = GetTenant(appAuth.AzureApp.Email);
            Uri url = new Uri($"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token");
            HttpClient httpClient = clientFactory.CreateClient();
            var buffer = await httpClient.PostAsync(url, formData);

            string json = await buffer.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(json);
            if (jObject.Property("access_token") != null)
            {
                logger.LogInformation($"授權名稱: {appAuth.Name}, Reflash Token: Success");
                appAuth.RefreshToken = jObject["refresh_token"].ToString();
                appAuth.UpdateTime = DateTime.Now;
                return (jObject["access_token"].ToString(), appAuth.Name);
            }

            logger.LogError(json);
            throw new BotException($"授權名稱: {appAuth.Name}, Reflash Token 失敗");
        }

        /// <summary>
        /// 取得 o365 使用者資訊
        /// </summary>
        /// <param name="token"> access token </param>
        /// <returns> User object </returns>
        public static async Task<User> GetUserInfoAsync(string token)
        {
            try
            {
                GraphServiceClient graphClient = GetGraphServiceClient(token);

                return await GetUserInfoAsync(graphClient);
            }
            catch
            {
                throw new BotException("獲取 User Info 失敗");
            }
        }

        /// <summary>
        /// 取得 o365 使用者資訊
        /// </summary>
        /// <param name="graphClient"> The object of GraphServiceClient. </param>
        /// <returns> User object </returns>
        public static async Task<User> GetUserInfoAsync(GraphServiceClient graphClient)
        {
            User user = await graphClient.Me
                .Request()
                .GetAsync();

            if (user.DisplayName != null && user.Mail != null)
                return user;

            throw new BotException("獲取 User Info 失敗");
        }

        /// <summary>
        /// 取得 Email 的 UPN 尾碼
        /// </summary>
        /// <param name="email"> 應用程式持有者的 email </param>
        /// <returns> UPN 尾碼 </returns>
        public static string GetTenant(string email)
        {
            return email.Split('@')[1];
        }
    }
}
