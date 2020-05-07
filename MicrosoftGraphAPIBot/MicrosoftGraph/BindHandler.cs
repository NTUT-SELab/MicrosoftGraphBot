using System;
using System.Text;
using System.Web;
using Telegram.Bot.Types;
using System.Net.Mail;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    public class BindHandler
    {
        private static readonly string appName = Guid.NewGuid().ToString();
        private const string appUrl = "https://localhost:44375/";

        public static string AppRegistrationUrl { 
            get {
                string ru = string.Format("https://developer.microsoft.com/en-us/graph/quick-start?appID=_appId_&appName=_appName_&redirectUrl={0}&platform=option-windowsuniversal", appUrl);
                string deeplink = string.Format("/quickstart/graphIO?publicClientSupport=false&appName={0}&redirectUrl={1}&allowImplicitFlow=true&ru=", appName, appUrl) + HttpUtility.UrlEncode(ru);
                return "https://apps.dev.microsoft.com/?deepLink=" + HttpUtility.UrlEncode(deeplink);
            } }

        /// <summary>
        /// 註冊 Azure 應用程式
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <param name="email"> 應用程式持有者的 email </param>
        /// <param name="clientId"> Application (client) ID </param>
        /// <param name="clientSecret"> Client secrets </param>
        /// <returns></returns>
        public async Task RegAppAsync(Message message, string email, string clientId, string clientSecret)
        {
            if (!IsValidEmail(email))
                throw new Exception("信箱格式錯誤");
            if (!Guid.TryParse(clientId, out _))
                throw new Exception("應用程式 Client Id 格式錯誤");
            if (!await IsValidApplicationAsync(email, clientId, clientSecret))
                throw new Exception("無效的 Azure 應用程式");

            // do something
        }

        /// <summary>
        /// 驗證是否為有效的 email 格式
        /// </summary>
        /// <param name="email"> 應用程式持有者的 email </param>
        /// <returns> True 為有效的 email 格式，False 為無效的 email 格式 </returns>
        private bool IsValidEmail(string email)
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
            using WebClient webClient = new WebClient();

            NameValueCollection body = new NameValueCollection()
            {
                { "grant_type", "client_credentials" },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "resource", "https://rest.media.azure.net" }
            };

            string domain = email.Split('@')[1];
            string url = string.Format("https://login.microsoftonline.com/{0}/oauth2/token", domain);
            byte[] buffer = await webClient.UploadValuesTaskAsync(url, body);
            string json = Encoding.UTF8.GetString(buffer);

            JObject jObject = JObject.Parse(json);

            if (jObject.Property("access_token") != null)
                return true;
            return false;
        }
    }
}
