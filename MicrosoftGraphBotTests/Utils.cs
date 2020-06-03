using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    public static class Utils
    {
        public static async Task<string> GetTestToken()
        {
            Uri tokenUrl = new Uri("https://raw.githubusercontent.com/NTUT-SELab/MicrosoftGraphToken/master/Token.txt");

            using HttpClient httpClient = new HttpClient();
            string json = await httpClient.GetStringAsync(tokenUrl);
            string reflashToken = JObject.Parse(json)["access_token"].ToString();

            return reflashToken;
        }
    }
}
