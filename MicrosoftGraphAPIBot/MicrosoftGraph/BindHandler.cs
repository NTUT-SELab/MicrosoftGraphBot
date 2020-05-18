using System;
using System.Web;
using System.Net.Mail;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using MicrosoftGraphAPIBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    /// <summary>
    /// 處理 o365 帳號綁定相關行為
    /// </summary>
    public class BindHandler
    {
        private static readonly string appName = Guid.NewGuid().ToString();
        public const string appUrl = "https://localhost:44375/";
        private readonly BotDbContext db;
        private readonly HttpClient httpClient;
        private const string Scope = "offline_access user.read";

        public static string AppRegistrationUrl { 
            get {
                string ru = $"https://developer.microsoft.com/en-us/graph/quick-start?appID=_appId_&appName=_appName_&redirectUrl={appUrl}&platform=option-windowsuniversal";
                string deeplink = $"/quickstart/graphIO?publicClientSupport=false&appName={appName}&redirectUrl={appUrl}&allowImplicitFlow=true&ru=" + HttpUtility.UrlEncode(ru);
                return "https://apps.dev.microsoft.com/?deepLink=" + HttpUtility.UrlEncode(deeplink);
            } }

        /// <summary>
        /// Create a new BindHandler instance.
        /// </summary>
        /// <param name="botDbContext"> Data base </param>
        /// <param name="httpClient"> 
        /// Provides a base class for sending HTTP requests and receiving HTTP responses
        /// from a resource identified by a URI.
        /// </param>
        public BindHandler(BotDbContext botDbContext, HttpClient httpClient)
        {
            this.db = botDbContext;
            this.httpClient = httpClient;
        }

        public async Task<string> GetAuthUrlAsync(Guid clientId)
        {
            string email = await db.AzureApps.Where(app => app.Id == clientId).Select(app => app.Email).FirstAsync();
            string url = $"https://login.microsoftonline.com/{GetTenant(email)}/oauth2/v2.0/authorize?client_id={clientId.ToString()}&response_type=code&redirect_uri={HttpUtility.UrlEncode(appUrl)}&response_mode=query&scope={HttpUtility.UrlEncode(Scope)}";
            return url;
        }

        /// <summary>
        /// 註冊 Azure 應用程式
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <param name="userName"> Telegram user name </param>
        /// <param name="email"> 應用程式持有者的 email </param>
        /// <param name="clientId"> Application (client) ID </param>
        /// <param name="clientSecret"> Client secrets </param>
        /// <returns></returns>
        public async Task RegAppAsync(long userId, string userName, string email, string clientId, string clientSecret)
        {
            if (!IsValidEmail(email))
                throw new InvalidOperationException("信箱格式錯誤");
            if (!Guid.TryParse(clientId, out Guid appId))
                throw new InvalidOperationException("應用程式 Client Id 格式錯誤");
            if (!await IsValidApplicationAsync(email, clientId, clientSecret))
                throw new InvalidOperationException("無效的 Azure 應用程式");

            // 寫入資料庫
            var telegramUser = db.TelegramUsers.Find(userId);
            if (telegramUser is null)
            {
                telegramUser = new TelegramUser { Id = userId, UserName = userName };
                db.TelegramUsers.Add(telegramUser);
            }
            db.AzureApps.Add(new AzureApp { 
                Id = appId,
                Secrets = clientId,
                Email = email,
                TelegramUser = telegramUser
            });

            // https://docs.microsoft.com/zh-tw/ef/core/saving/explicit-values-generated-properties#explicit-values-into-sql-server-identity-columns
            await db.Database.OpenConnectionAsync();
            db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.TelegramUsers ON");
            db.SaveChanges();
            db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.TelegramUsers OFF");
            await db.Database.CloseConnectionAsync();
        }

        /// <summary>
        /// 取得指定 Telegram 使用者註冊的應用程式數量
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <returns> 應用程式數量 </returns>
        public async Task<int> AppCountAsync(long userId)
        {
            return await db.AzureApps
                .Where(app => app.TelegramUser.Id == userId)
                .CountAsync();
        }

        /// <summary>
        /// 取得指定 Telegram 使用者註冊的應用程式 Id
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <returns> 應用程式Guid </returns>
        public async Task<IEnumerable<(Guid, DateTime)>> GetAppsInfoAsync(long userId)
        {
            var appInfo = await db.AzureApps
                .Where(app => app.TelegramUser.Id == userId)
                .Select(app => new { app.Id, app.Date })
                .ToListAsync();
            return appInfo.Select(app => (app.Id, app.Date));
        }

        /// <summary>
        /// 驗證是否為有效的 email 格式
        /// </summary>
        /// <param name="email"> 應用程式持有者的 email </param>
        /// <returns> True 為有效的 email 格式，False 為無效的 email 格式 </returns>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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
        private async Task<bool> IsValidApplicationAsync(string email, string clientId, string clientSecret)
        {
            Dictionary<string, string> body = new Dictionary<string, string>()
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "resource", "https://rest.media.azure.net" }
            };

            var formData = new FormUrlEncodedContent(body);
            string tenant = GetTenant(email);
            string url = $"https://login.microsoftonline.com/{tenant}/oauth2/token";
            var buffer = await httpClient.PostAsync(url, formData);

            string json = await buffer.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(json);
            if (jObject.Property("access_token") != null)
                return true;

            return false;
        }

        /// <summary>
        /// 取得 Email 的 UPN 尾碼
        /// </summary>
        /// <param name="email"> 應用程式持有者的 email </param>
        /// <returns> UPN 尾碼 </returns>
        private static string GetTenant(string email)
        {
            return email.Split('@')[1];
        }
    }
}
