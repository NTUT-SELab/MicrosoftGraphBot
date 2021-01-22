using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSGraphAuthApi.Controllers;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class MSGraphAuthApiTests
    {
        private readonly AuthCodeController authCodeController;

        public MSGraphAuthApiTests()
        {
            authCodeController = new AuthCodeController();
        }

        [TestMethod]
        public void TestGetAsync()
        {
            string content = authCodeController.Get(null, null);

            Assert.AreEqual("https://github.com/NTUT-SELab/MicrosoftGraphBot", content);
        }

        [TestMethod]
        public void TestGetCodeAsync()
        {
            Guid id = Guid.NewGuid();
            string json = authCodeController.Get(id.ToString(), null);
            JObject jObject = JObject.Parse(json);

            if (jObject.Property("code") == null)
            {
                Assert.Fail("Code parameter is null");
            }

            Assert.AreEqual(id.ToString(), jObject["code"]);
        }

        [TestMethod]
        public void TestGetErrorAsync()
        {
            Guid id = Guid.NewGuid();
            string json = authCodeController.Get(null, id.ToString());
            JObject jObject = JObject.Parse(json);

            if (jObject.Property("error") == null)
            {
                Assert.Fail("Error parameter is null");
            }

            Assert.AreEqual(id.ToString(), jObject["error"]);
        }
    }

    [TestClass]
    public class MSGraphAuthApiUseRequestTests
    {
        private readonly HttpClient httpClient;

        public MSGraphAuthApiUseRequestTests()
        {
            var webAppFixture = new WebApplicationFactory<MSGraphAuthApi.Startup>();
            httpClient = webAppFixture.CreateClient();
        }

        [TestMethod]
        public async Task TestRootAsync()
        {
            var buffer = await httpClient.GetAsync("/");
            string content = await buffer.Content.ReadAsStringAsync();

            Assert.AreEqual("https://github.com/NTUT-SELab/MicrosoftGraphBot", content);
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
