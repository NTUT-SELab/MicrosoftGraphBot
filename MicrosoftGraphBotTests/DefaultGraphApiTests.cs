using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using MicrosoftGraphAPIBot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class DefaultGraphApiTests
    {
        [TestMethod]
        public async Task TestIsValidApplicationAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "ValidApplicationResult.json");
            string json = File.ReadAllText(testResultPath);
            Guid clientId = Guid.NewGuid();
            var mocks = Utils.CreateDefaultGraphApiMock(json);
            await Utils.SetOneValueDbContextAsync(clientId);
            BotDbContext db = Utils.CreateMemoryDbContext();

            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);
            Assert.IsTrue(await defaultGraphApi.IsValidApplicationAsync("test@onmicrosoft.com", string.Empty, string.Empty));
        }

        [TestMethod]
        public async Task TestIsInvalidApplicationAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "InvalidApplicationResult.json");
            string json = File.ReadAllText(testResultPath);
            Guid clientId = Guid.NewGuid();
            var mocks = Utils.CreateDefaultGraphApiMock(json);
            await Utils.SetOneValueDbContextAsync(clientId);
            BotDbContext db = Utils.CreateMemoryDbContext();

            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);
            Assert.IsFalse(await defaultGraphApi.IsValidApplicationAsync("test@onmicrosoft.com", string.Empty, string.Empty));
        }

        [TestMethod]
        public async Task TestGetTokenSuccessAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "GetTokenSuccessResult.json");
            string json = File.ReadAllText(testResultPath);
            Guid clientId = Guid.NewGuid();
            var mocks = Utils.CreateDefaultGraphApiMock(json);
            await Utils.SetOneValueDbContextAsync(clientId);
            BotDbContext db = Utils.CreateMemoryDbContext();

            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);
            (string, string) tokens = await defaultGraphApi.GetTokenAsync(clientId, string.Empty);

            JObject jObject = JObject.Parse(json);
            Assert.AreEqual(jObject["access_token"].ToString(), tokens.Item1);
            Assert.AreEqual(jObject["refresh_token"].ToString(), tokens.Item2);
        }

        [ExpectedException(typeof(BotException))]
        [TestMethod]
        public async Task TestGetTokenFailedAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "GetTokenFailedResult.json");
            string json = File.ReadAllText(testResultPath);
            Guid clientId = Guid.NewGuid();
            var mocks = Utils.CreateDefaultGraphApiMock(json);
            await Utils.SetOneValueDbContextAsync(clientId);
            BotDbContext db = Utils.CreateMemoryDbContext();

            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);
            (string, string) _ = await defaultGraphApi.GetTokenAsync(clientId, string.Empty);
        }

        [ExpectedException(typeof(BotException))]
        [TestMethod]
        public async Task TestGetTokenAzureAppNotFoundAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "GetTokenSuccessResult.json");
            string json = File.ReadAllText(testResultPath);
            Guid clientId = Guid.NewGuid();
            var mocks = Utils.CreateDefaultGraphApiMock(json);
            BotDbContext db = Utils.CreateMemoryDbContext();

            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);
            (string, string) _ = await defaultGraphApi.GetTokenAsync(clientId, string.Empty);
        }

        [TestMethod]
        public async Task TestReflashTokenSuccessAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "GetTokenSuccessResult.json");
            string json = File.ReadAllText(testResultPath);
            Guid clientId = Guid.NewGuid();
            var mocks = Utils.CreateDefaultGraphApiMock(json);
            await Utils.SetOneValueDbContextAsync(clientId);
            BotDbContext db = Utils.CreateMemoryDbContext();
            AppAuth appAuth = await db.AppAuths.Include(appAuth => appAuth.AzureApp).FirstAsync();

            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);
            (string, string) tokens = await defaultGraphApi.ReflashTokenAsync(appAuth);

            JObject jObject = JObject.Parse(json);
            Assert.AreEqual(jObject["access_token"].ToString(), tokens.Item1);
            Assert.AreEqual(appAuth.Name, tokens.Item2);
            Assert.AreEqual(jObject["refresh_token"].ToString(), appAuth.RefreshToken);
        }

        [ExpectedException(typeof(BotException))]
        [TestMethod]
        public async Task TestReflashTokenFailedAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "GetTokenFailedResult.json");
            string json = File.ReadAllText(testResultPath);
            Guid clientId = Guid.NewGuid();
            var mocks = Utils.CreateDefaultGraphApiMock(json);
            await Utils.SetOneValueDbContextAsync(clientId);
            BotDbContext db = Utils.CreateMemoryDbContext();
            AppAuth appAuth = await db.AppAuths.Include(appAuth => appAuth.AzureApp).FirstAsync();

            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);
            (string, string) _ = await defaultGraphApi.ReflashTokenAsync(appAuth);
        }

        [TestMethod]
        public async Task TeskGetUserInfoSuccessAsync()
        {
            string token = await Utils.GetTestToken();
            Assert.IsNotNull(await DefaultGraphApi.GetUserInfoAsync(token));
        }

        [ExpectedException(typeof(BotException))]
        [TestMethod]
        public async Task TeskGetUserInfoFailedAsync()
        {
            _ = await DefaultGraphApi.GetUserInfoAsync(string.Empty);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await Utils.DeleteDBAsync();
        }
    }
}
