using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using MicrosoftGraphAPIBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class BindHandlerTests
    {
        [TestMethod]
        public async Task TestGetAuthUrlAsync()
        {
            Guid clientId = Guid.NewGuid();
            var mocks = Utils.CreateDefaultGraphApiMock(string.Empty, clientId);
            await Utils.SetOneValueDbContextAsync(clientId);
            BotDbContext db = Utils.CreateMemoryDbContext();
            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);

            BindHandler bindHandler = new BindHandler(db, defaultGraphApi);
            (string, string) results = await bindHandler.GetAuthUrlAsync(clientId.ToString());

            string email = "test@onmicrosoft.com";
            string message = $"https://login.microsoftonline.com/{DefaultGraphApi.GetTenant(email)}/oauth2/v2.0/authorize?client_id={clientId}&response_type=code&redirect_uri={HttpUtility.UrlEncode(BindHandler.AppUrl)}&response_mode=query&scope={HttpUtility.UrlEncode(DefaultGraphApi.Scope)}";

            Assert.AreEqual(message, results.Item2);
        }

        [TestMethod]
        public async Task TestRegAppAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "ValidApplicationResult.json");
            string json = File.ReadAllText(testResultPath);
            var mocks = Utils.CreateDefaultGraphApiMock(json, Guid.Empty);
            BotDbContext db = Utils.CreateMemoryDbContext();
            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);

            BindHandler bindHandler = new BindHandler(db, defaultGraphApi);

            long userId = 123456;
            string userName = "Test Bot";
            string email = "test@onmicrosoft.com";
            Guid clientId = Guid.NewGuid();
            string clientSecret = "741852963";

            await bindHandler.RegAppAsync(userId, userName, email, clientId.ToString(), clientSecret);
            await db.DisposeAsync();
            db = Utils.CreateMemoryDbContext();
            AzureApp azureApp = await db.AzureApps.Include(azureApp => azureApp.TelegramUser).FirstAsync();
            Assert.AreEqual(userId, azureApp.TelegramUserId);
            Assert.AreEqual(userName, azureApp.TelegramUser.UserName);
            Assert.AreEqual(email, azureApp .Email);
            Assert.AreEqual(clientId, azureApp.Id);
            Assert.AreEqual(clientSecret, azureApp.Secrets);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task TestRegInvalidAppAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "InvalidApplicationResult.json");
            string json = File.ReadAllText(testResultPath);
            var mocks = Utils.CreateDefaultGraphApiMock(json, Guid.Empty);
            BotDbContext db = Utils.CreateMemoryDbContext();
            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);

            BindHandler bindHandler = new BindHandler(db, defaultGraphApi);

            long userId = 123456;
            string userName = "Test Bot";
            string email = "test@onmicrosoft.com";
            Guid clientId = Guid.NewGuid();
            string clientSecret = "741852963";

            await bindHandler.RegAppAsync(userId, userName, email, clientId.ToString(), clientSecret);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task TestRegAppInvalidEmailAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "InvalidApplicationResult.json");
            string json = File.ReadAllText(testResultPath);
            var mocks = Utils.CreateDefaultGraphApiMock(json, Guid.Empty);
            BotDbContext db = Utils.CreateMemoryDbContext();
            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);

            BindHandler bindHandler = new BindHandler(db, defaultGraphApi);

            long userId = 123456;
            string userName = "Test Bot";
            string email = "onmicrosoft.com";
            Guid clientId = Guid.NewGuid();
            string clientSecret = "741852963";

            await bindHandler.RegAppAsync(userId, userName, email, clientId.ToString(), clientSecret);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task TestRegAppInvalidClientIdAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "InvalidApplicationResult.json");
            string json = File.ReadAllText(testResultPath);
            var mocks = Utils.CreateDefaultGraphApiMock(json, Guid.Empty);
            BotDbContext db = Utils.CreateMemoryDbContext();
            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);

            BindHandler bindHandler = new BindHandler(db, defaultGraphApi);

            long userId = 123456;
            string userName = "Test Bot";
            string email = "test@onmicrosoft.com";
            string clientId = "1da2d3as3d1321ad3a";
            string clientSecret = "741852963";

            await bindHandler.RegAppAsync(userId, userName, email, clientId, clientSecret);
        }

        [TestMethod]
        public async Task TestAppCountAsync()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();

            BindHandler bindHandler = new BindHandler(db, null);
            Assert.AreEqual(2, await bindHandler.AppCountAsync(123456789));
        }

        [TestMethod]
        public async Task TestGetAppsInfoAsync()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();

            BindHandler bindHandler = new BindHandler(db, null);
            var appsInfo = (await bindHandler.GetAppsInfoAsync(123456789)).ToList();
            Assert.IsTrue((appsInfo[0].Item2 - DateTime.Now).TotalSeconds < 1);
            Assert.IsTrue((appsInfo[1].Item2 - DateTime.Now).TotalSeconds < 1);
        }

        [TestMethod]
        public async Task TestBindAuthAsync()
        {
            string token = await Utils.GetTestToken();
            string authResponsePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "AuthResponse.txt");
            string authResponse = File.ReadAllText(authResponsePath);
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "GetTokenSuccessResult.json");
            string json = File.ReadAllText(testResultPath);
            JObject jObject = JObject.Parse(json);
            jObject["access_token"] = token;
            json = JsonConvert.SerializeObject(jObject);
            Guid clientId = Guid.NewGuid();
            var mocks = Utils.CreateDefaultGraphApiMock(json, clientId);
            await Utils.SetOneValueDbContextAsync(clientId);
            BotDbContext db = Utils.CreateMemoryDbContext();
            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(db, mocks.Item1, mocks.Item2);

            string name = "test Bind";
            BindHandler bindHandler = new BindHandler(db, defaultGraphApi);
            await bindHandler.BindAuthAsync(clientId.ToString(), authResponse, name);
            await db.DisposeAsync();
            db = Utils.CreateMemoryDbContext();
            Assert.AreEqual(2, await db.AppAuths.AsQueryable().CountAsync());
            Assert.IsTrue(await db.AppAuths.AsQueryable().AnyAsync(appAuth => appAuth.Name == name));
        }

        [ExpectedException(typeof(UriFormatException))]
        [TestMethod]
        public async Task TestBindAutErrorUrlAsync()
        {
            BindHandler bindHandler = new BindHandler(null, null);
            await bindHandler.BindAuthAsync(Guid.Empty.ToString(), string.Empty, string.Empty);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await Utils.DeleteDBAsync();
        }
    }
}
