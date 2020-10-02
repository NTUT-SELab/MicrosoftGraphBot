using System;
using System.Web;
using System.Threading.Tasks;
using MicrosoftGraphAPIBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    /// <summary>
    /// 處理 o365 帳號綁定相關行為
    /// </summary>
    public class BindHandler
    {
        public const string AppUrl = "https://msgraphauthorization.azurewebsites.net/authcode/";
        public const string DeleteUrl = "https://portal.azure.com/#blade/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/Overview/appId/{0}/isMSAApp/";
        private readonly BotDbContext db;
        private readonly DefaultGraphApi defaultGraphApi;

        public static string AppRegistrationUrl
        {
            get
            {
                Guid appName = Guid.NewGuid();
                string ru = $"https://developer.microsoft.com/en-us/graph/quick-start?appID=_appId_&appName=_appName_&redirectUrl={AppUrl}&platform=option-windowsuniversal";
                string deeplink = $"/quickstart/graphIO?publicClientSupport=false&appName={appName}&redirectUrl={AppUrl}&allowImplicitFlow=true&ru=" + HttpUtility.UrlEncode(ru);
                return "https://apps.dev.microsoft.com/?deepLink=" + HttpUtility.UrlEncode(deeplink);
            }
        }

        /// <summary>
        /// Create a new BindHandler instance.
        /// </summary>
        /// <param name="botDbContext"></param>
        /// <param name="defaultGraphApi"></param>
        public BindHandler(BotDbContext botDbContext, DefaultGraphApi defaultGraphApi) =>
            (this.db, this.defaultGraphApi) = (botDbContext, defaultGraphApi);

        /// <summary>
        /// 取得 o365 授權網址
        /// </summary>
        /// <param name="Id"> Application (client) ID or Auth ID </param>
        /// <param name="isClientId"> Id 參數是否為 Application (client) ID </param>
        /// <returns> (clientId, o365 授權網址) </returns>
        public async Task<(string, string)> GetAuthUrlAsync(string Id, bool isClientId = true)
        {
            try
            {
                string clientId = string.Empty;
                string email = string.Empty;
                string authId = string.Empty;

                if (isClientId)
                {
                    clientId = Id;
                    email = await db.AzureApps.AsQueryable().Where(app => app.Id == Guid.Parse(clientId)).Select(app => app.Email).FirstAsync();
                }
                else
                {
                    var results = await db.AppAuths.Include(auth => auth.AzureApp).Where(auth => auth.Id == Guid.Parse(Id)).Select(auth => new { auth.AzureAppId, auth.AzureApp.Email, auth.Id }).FirstAsync();
                    (clientId, email, authId) = (results.AzureAppId.ToString(), results.Email, results.Id.ToString());
                }
                string url = $"https://login.microsoftonline.com/{DefaultGraphApi.GetTenant(email)}/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&redirect_uri={HttpUtility.UrlEncode(AppUrl)}&response_mode=query&scope={HttpUtility.UrlEncode(DefaultGraphApi.Scope)}";
                if (isClientId)
                    return (clientId, url);
                return (authId, url);
            }
            catch (InvalidOperationException ex) when (ex.Message == "Sequence contains no elements")
            {
                throw new BotException("無效的應用程式或授權");
            }
        }

        #region Azure app

        /// <summary>
        /// 註冊 Azure 應用程式
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <param name="userName"> Telegram user name </param>
        /// <param name="email"> 應用程式持有者的 email </param>
        /// <param name="clientId"> Application (client) ID </param>
        /// <param name="clientSecret"> Client secrets </param>
        /// <returns></returns>
        public async Task RegAppAsync(long userId, string userName, string email, string clientId, string clientSecret, string appName)
        {
            if (!IsValidEmail(email))
                throw new BotException("信箱格式錯誤");
            if (!Guid.TryParse(clientId, out Guid appId))
                throw new BotException("應用程式 Client Id 格式錯誤");
            if (!await defaultGraphApi.IsValidApplicationAsync(email, clientId, clientSecret))
                throw new BotException("無效的 Azure 應用程式");

            // 寫入資料庫
            var telegramUser = db.TelegramUsers.Find(userId);
            if (telegramUser is null)
            {
                telegramUser = new TelegramUser { Id = userId, UserName = userName };
                db.TelegramUsers.Add(telegramUser);
            }
            db.AzureApps.Add(new AzureApp { 
                Id = appId,
                Name = appName,
                Secrets = clientSecret,
                Email = email,
                TelegramUser = telegramUser
            });

            if (!db.Database.IsInMemory())
            {
                // https://docs.microsoft.com/zh-tw/ef/core/saving/explicit-values-generated-properties#explicit-values-into-sql-server-identity-columns
                await db.Database.OpenConnectionAsync();
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.TelegramUsers ON");
                db.SaveChanges();
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.TelegramUsers OFF");
                await db.Database.CloseConnectionAsync();
            }
            else
                await db.SaveChangesAsync();
        }

        /// <summary>
        /// 刪除 Azure 應用程式
        /// </summary>
        /// <param name="clientId"> Application (client) ID </param>
        /// <returns></returns>
        public async Task<string> DeleteAppAsync(string clientId)
        {
            AzureApp azureApp = await db.AzureApps.Include(app => app.AppAuths).FirstOrDefaultAsync(app => app.Id == Guid.Parse(clientId));
            if (azureApp == null)
                throw new BotException("此 Azure 應用程式不存在");
            db.Remove(azureApp);
            await db.SaveChangesAsync();

            return string.Format(DeleteUrl, clientId);
        }

        #endregion

        #region App auth

        /// <summary>
        /// 對指定應用程式取得 o365 帳號授權
        /// </summary>
        /// <param name="clientId"> Application (client) ID </param>
        /// <param name="json"> 含有 Code 訊息的 json 字串 </param>
        /// <param name="name"> 授權別名 </param>
        /// <returns></returns>
        public async Task BindAuthAsync(string clientId, string json, string name)
        {

            JObject jObject;
            try
            {
                jObject = JObject.Parse(json);
            }
            catch
            {
                throw new BotException("網頁內容格式錯誤");
            }
            if (jObject.Property("code") == null && jObject.Property("error") != null)
                throw new BotException(jObject["error"].ToString());
            if (jObject.Property("code") == null && jObject.Property("error") == null)
                throw new BotException("網頁內容缺少必要訊息");
            Guid appId = Guid.Parse(clientId);
            (string, string) tokens = await defaultGraphApi.GetTokenAsync(appId, jObject["code"].ToString());
            await DefaultGraphApi.GetUserInfoAsync(tokens.Item1);

            db.AppAuths.Add(new AppAuth { 
                AzureAppId = appId,
                Name = name,
                RefreshToken = tokens.Item2,
                Scope = DefaultGraphApi.Scope
            });

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// 解除指定應用程式的 o365 帳號授權
        /// </summary>
        /// <param name="clientId"> Application (client) ID </param>
        /// <returns></returns>
        public async Task UnbindAuthAsync(string authId)
        {
            AppAuth appAuth = new AppAuth { Id = Guid.Parse(authId) };
            db.Remove(appAuth);
            try
            {
                await db.SaveChangesAsync();
            }
            catch
            {
                throw new BotException("此 o365 授權不存在");
            }
        }

        /// <summary>
        /// 對指定應用程式更新 o365 帳號授權
        /// </summary>
        /// <param name="authId"> 授權 Id </param>
        /// <param name="json"> 含有 Code 訊息的 json 字串 </param>
        /// <returns></returns>
        public async Task UpdateAuthAsync(string authId, string json)
        {

            JObject jObject;
            try
            {
                jObject = JObject.Parse(json);
            }
            catch
            {
                throw new BotException("網頁內容格式錯誤");
            }
            if (jObject.Property("code") == null && jObject.Property("error") != null)
                throw new BotException(jObject["error"].ToString());
            if (jObject.Property("code") == null && jObject.Property("error") == null)
                throw new BotException("網頁內容缺少必要訊息");
            AppAuth auth = await db.AppAuths.FindAsync(Guid.Parse(authId));

            if (auth == null)
                throw new BotException("授權不存在");

            Guid appId = auth.AzureAppId;
            (string, string) tokens = await defaultGraphApi.GetTokenAsync(appId, jObject["code"].ToString());
            await DefaultGraphApi.GetUserInfoAsync(tokens.Item1);

            auth.RefreshToken = tokens.Item2;
            auth.UpdateTime = DateTime.Now;
            auth.Scope = DefaultGraphApi.Scope;

            db.AppAuths.Update(auth);

            await db.SaveChangesAsync();
        }

        #endregion

        /// <summary>
        /// 驗證是否為有效的 email 格式
        /// 
        /// https://docs.microsoft.com/zh-tw/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
        /// </summary>
        /// <param name="email"> 應用程式持有者的 email </param>
        /// <returns> True 為有效的 email 格式，False 為無效的 email 格式 </returns>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                static string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    var domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
    }
}
