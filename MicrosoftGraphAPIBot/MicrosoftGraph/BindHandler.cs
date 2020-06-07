using System;
using System.Web;
using System.Threading.Tasks;
using MicrosoftGraphAPIBot.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

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
        private readonly DefaultGraphApi defaultGraphApi;

        public static string AppRegistrationUrl { 
            get {
                string ru = $"https://developer.microsoft.com/en-us/graph/quick-start?appID=_appId_&appName=_appName_&redirectUrl={appUrl}&platform=option-windowsuniversal";
                string deeplink = $"/quickstart/graphIO?publicClientSupport=false&appName={appName}&redirectUrl={appUrl}&allowImplicitFlow=true&ru=" + HttpUtility.UrlEncode(ru);
                return "https://apps.dev.microsoft.com/?deepLink=" + HttpUtility.UrlEncode(deeplink);
            } }

        /// <summary>
        /// Create a new BindHandler instance.
        /// </summary>
        /// <param name="botDbContext"></param>
        /// <param name="defaultGraphApi"></param>
        public BindHandler(BotDbContext botDbContext, DefaultGraphApi defaultGraphApi)
        {
            this.db = botDbContext;
            this.defaultGraphApi = defaultGraphApi;
        }

        /// <summary>
        /// 取得 o365 授權網址
        /// </summary>
        /// <param name="clientId"> Application (client) ID </param>
        /// <returns></returns>
        public async Task<(string, string)> GetAuthUrlAsync(string clientId)
        {
            string email = await db.AzureApps.AsQueryable().Where(app => app.Id == Guid.Parse(clientId)).Select(app => app.Email).FirstAsync();
            string url = $"https://login.microsoftonline.com/{DefaultGraphApi.GetTenant(email)}/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&redirect_uri={HttpUtility.UrlEncode(appUrl)}&response_mode=query&scope={HttpUtility.UrlEncode(DefaultGraphApi.Scope)}";
            return (clientId, url);
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
            if (!await defaultGraphApi.IsValidApplicationAsync(email, clientId, clientSecret))
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
                Secrets = clientSecret,
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
            return await db.AzureApps.AsQueryable()
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
            var appInfo = await db.AzureApps.AsQueryable()
                .Where(app => app.TelegramUser.Id == userId)
                .Select(app => new { app.Id, app.Date })
                .ToListAsync();
            return appInfo.Select(app => (app.Id, app.Date));
        }

        /// <summary>
        /// 對指定應用程式取得 o365 帳號授權
        /// </summary>
        /// <param name="clientId"> Application (client) ID </param>
        /// <param name="code"> The authorization_code that the app requested. 
        /// The app can use the authorization code to request an access token for the target resource. 
        /// Authorization_codes are very short lived, typically they expire after about 10 minutes. </param>
        /// <param name="name"> 授權別名 </param>
        /// <returns></returns>
        public async Task BindAuth(string clientId, string authResponse, string name)
        {
            Uri responseUrl = new Uri(authResponse);
            string code = HttpUtility.ParseQueryString(responseUrl.Query).Get("code");
            Guid appId = Guid.Parse(clientId);
            (string, string) tokens = await defaultGraphApi.GetTokenAsync(appId, code);
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
