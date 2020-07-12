using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class MSGraphAuthApiTests
    {
        private readonly HttpClient httpClient;

        public MSGraphAuthApiTests()
        {
            var webAppFixture = new WebApplicationFactory<MSGraphAuthApi.Startup>();
            httpClient = webAppFixture.CreateClient();
        }

        [TestMethod]
        public async Task TestGetAsync()
        {
            var buffer = await httpClient.GetAsync("/AuthCode");
            string content = await buffer.Content.ReadAsStringAsync();

            Assert.AreEqual("https://github.com/NTUT-SELab/MicrosoftGraphBot", content);
        }

        [TestMethod]
        public async Task TestGetCodeAsync()
        {
            Guid id = Guid.NewGuid();
            var buffer = await httpClient.GetAsync($"/AuthCode/?Code={id}");
            string json = await buffer.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(json);

            if (jObject.Property("code") == null)
            {
                Assert.Fail("Code parameter is null");
            }

            Assert.AreEqual(id.ToString(), jObject["code"]);
        }

        [TestMethod]
        public async Task TestGetErrorAsync()
        {
            Guid id = Guid.NewGuid();
            var buffer = await httpClient.GetAsync($"/AuthCode/?Error={id}");
            string json = await buffer.Content.ReadAsStringAsync();
            JObject jObject = JObject.Parse(json);

            if (jObject.Property("error") == null)
            {
                Assert.Fail("Error parameter is null");
            }

            Assert.AreEqual(id.ToString(), jObject["error"]);
        }
    }
}
