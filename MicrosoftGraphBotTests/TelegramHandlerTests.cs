using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using MicrosoftGraphAPIBot.Models;
using MicrosoftGraphAPIBot.Telegram;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class TelegramHandlerTests
    {
        private readonly ILogger<TelegramHandler> logger;
        private readonly IConfiguration configuration;

        public TelegramHandlerTests()
        {
            Mock loggerMock = new Mock<ILogger<TelegramHandler>>();
            this.logger = loggerMock.Object as ILogger<TelegramHandler>;
            Dictionary<string, string> config = new Dictionary<string, string>
            {
                {"AdminPassword", "P@ssw0rd"}
            };

            this.configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();
        }

        [TestMethod]
        public async Task TestAddAdminPermissionSuccess()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            bool result = await telegramHandler.AddAdminPermission(123456789, "Bot1", "P@ssw0rd");

            await db.DisposeAsync();
            db = Utils.CreateMemoryDbContext();
            TelegramUser user = await db.TelegramUsers.FindAsync((long)123456789);

            Assert.IsTrue(result);
            Assert.AreEqual("Bot1", user.UserName);
            Assert.IsTrue(user.IsAdmin);
        }

        [TestMethod]
        public async Task TestAddAdminPermissionSuccessNoUser()
        {
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            bool result = await telegramHandler.AddAdminPermission(123456789, "Bot1", "P@ssw0rd");

            await db.DisposeAsync();
            db = Utils.CreateMemoryDbContext();
            TelegramUser user = await db.TelegramUsers.FindAsync((long)123456789);

            Assert.IsTrue(result);
            Assert.AreEqual(1, await db.TelegramUsers.AsQueryable().CountAsync());
            Assert.AreEqual("Bot1", user.UserName);
            Assert.IsTrue(user.IsAdmin);
        }

        [TestMethod]
        public async Task TestAddAdminPermissionFailed()
        {
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            bool result = await telegramHandler.AddAdminPermission(123456789, "Bot1", "@@@@@@@");

            await db.DisposeAsync();
            db = Utils.CreateMemoryDbContext();

            Assert.IsFalse(result);
            Assert.AreEqual(0, await db.TelegramUsers.AsQueryable().CountAsync());
        }

        [TestMethod]
        public async Task TestRemoveAdminPermission()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            await telegramHandler.RemoveAdminPermission(123456789, "Bot1");

            await db.DisposeAsync();
            db = Utils.CreateMemoryDbContext();
            TelegramUser user = await db.TelegramUsers.FindAsync((long)123456789);

            Assert.AreEqual("Bot1", user.UserName);
            Assert.IsFalse(user.IsAdmin);
        }

        [TestMethod]
        public async Task TestRemoveAdminPermissionNoUser()
        {
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            await telegramHandler.RemoveAdminPermission(123456789, "Bot1");

            await db.DisposeAsync();
            db = Utils.CreateMemoryDbContext();
            TelegramUser user = await db.TelegramUsers.FindAsync((long)123456789);

            Assert.AreEqual(1, await db.TelegramUsers.AsQueryable().CountAsync());
            Assert.AreEqual("Bot1", user.UserName);
            Assert.IsFalse(user.IsAdmin);
        }

        [TestMethod]
        public async Task TestCheckIsAdminTrue()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            await telegramHandler.AddAdminPermission(123456789, "Bot1", "P@ssw0rd");
            await db.DisposeAsync();
            db = Utils.CreateMemoryDbContext();
            telegramHandler = new TelegramHandler(logger, configuration, db);
            bool result = await telegramHandler.CheckIsAdmin(123456789);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCheckIsAdminFalse()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            bool result = await telegramHandler.CheckIsAdmin(123456789);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TestCheckIsAdminNoUser()
        {
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            bool result = await telegramHandler.CheckIsAdmin(123456789);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task TestAppCountAsync()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            Assert.AreEqual(2, await telegramHandler.AppCountAsync(123456789));
        }

        [TestMethod]
        public async Task TestAuthCountAsync()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            Assert.AreEqual(1, await telegramHandler.AuthCountAsync(123456789));
        }

        [TestMethod]
        public async Task TestGetAppsNameAsync()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            var appsInfo = (await telegramHandler.GetAppsNameAsync(123456789)).ToList();
            Assert.AreEqual(appsInfo.Count(), 2);
            Assert.AreEqual(appsInfo[0].Item2, "App1");
            Assert.AreEqual(appsInfo[1].Item2, "App2");
        }

        [TestMethod]
        public async Task TestGetAppInfoAsync()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();
            Guid clientId1 = await db.AzureApps.AsQueryable().Select(app => app.Id).FirstAsync();
            await db.DisposeAsync();
            db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            var appInfo = (await telegramHandler.GetAppInfoAsync(clientId1.ToString()));
            Assert.AreEqual(appInfo.Name, "App1");
            Assert.AreEqual(appInfo.Email, "test@onmicrosoft.com");
            Assert.AreEqual(appInfo.Secrets, string.Empty);
            Assert.AreEqual(appInfo.TelegramUserId, 123456789);
            Assert.AreEqual(appInfo.Id, clientId1);
        }

        [TestMethod]
        public async Task TestGetAuthsNameAsync()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            var authsInfo = (await telegramHandler.GetAuthsNameAsync(123456789)).ToList();
            Assert.AreEqual(authsInfo.Count(), 1);
            Assert.AreEqual(authsInfo[0].Item2, "Auth1");
        }

        [TestMethod]
        public async Task TestGetAuthInfoAsync()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();
            Guid authId1 = await db.AppAuths.AsQueryable().Select(app => app.Id).FirstAsync();
            await db.DisposeAsync();
            db = Utils.CreateMemoryDbContext();

            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            var authInfo = (await telegramHandler.GetAuthInfoAsync(authId1.ToString()));
            Assert.AreEqual(authInfo.Name, "Auth1");
            Assert.AreEqual(authInfo.RefreshToken, string.Empty);
            Assert.AreEqual(authInfo.Scope, DefaultGraphApi.Scope);
            Assert.AreEqual(authInfo.Id, authId1);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await Utils.DeleteDBAsync();
        }
    }
}
