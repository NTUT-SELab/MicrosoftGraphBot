using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.Models;
using MicrosoftGraphAPIBot.Telegram;
using Moq;
using System.Collections.Generic;
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

            configuration = new ConfigurationBuilder()
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
            Assert.AreEqual(1, await db.TelegramUsers.CountAsync());
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
            Assert.AreEqual(0, await db.TelegramUsers.CountAsync());
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

            Assert.AreEqual(1, await db.TelegramUsers.CountAsync());
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

        [TestCleanup]
        public async Task Cleanup()
        {
            await Utils.DeleteDBAsync();
        }
    }
}
