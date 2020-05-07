using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    public static class BindHandler
    {
        private static readonly string appName = Guid.NewGuid().ToString();
        private const string appUrl = "https://localhost:44375/";

        public static string AppRegistrationUrl { 
            get {
                string ru = string.Format("https://developer.microsoft.com/en-us/graph/quick-start?appID=_appId_&appName=_appName_&redirectUrl={0}&platform=option-windowsuniversal", appUrl);
                string deeplink = string.Format("/quickstart/graphIO?publicClientSupport=false&appName={0}&redirectUrl={1}&allowImplicitFlow=true&ru=", appName, appUrl) + HttpUtility.UrlEncode(ru);
                return "https://apps.dev.microsoft.com/?deepLink=" + HttpUtility.UrlEncode(deeplink);
            } }
    }
}
