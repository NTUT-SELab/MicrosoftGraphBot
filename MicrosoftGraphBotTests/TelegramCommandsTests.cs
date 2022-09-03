using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    public class TelegramCommandsTests
    {
        private readonly ILogger<TelegramHandler> logger;
        private readonly IConfiguration configuration;

        public TelegramCommandsTests()
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
        public async Task TestGenerateMenuCommandsAsyncAuth()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();
            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            TelegramCommandGenerator telegramCommand = new TelegramCommandGenerator(telegramHandler);
            
            var result = await telegramCommand.GenerateMenuCommandsAsync(123456789);
            Assert.AreEqual(4, result.Count());
            Assert.AreEqual(1, result.Where(items => items.Item1 == TelegramCommand.AddAdminPermission).Count());
        }

        [TestMethod]
        public async Task TestGenerateMenuCommandsAsyncNoAuth()
        {
            BotDbContext db = Utils.CreateMemoryDbContext();
            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            TelegramCommandGenerator telegramCommand = new TelegramCommandGenerator(telegramHandler);

            int count = (await telegramCommand.GenerateMenuCommandsAsync(123456789)).Count();
            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public async Task TestGenerateMenuCommandsAsyncAdmin()
        {
            BotDbContext db = Utils.CreateMemoryDbContext();
            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            TelegramCommandGenerator telegramCommand = new TelegramCommandGenerator(telegramHandler);

            await telegramHandler.AddAdminPermissionAsync(123456789, "Bot1", "P@ssw0rd");
            var result = await telegramCommand.GenerateMenuCommandsAsync(123456789);
            Assert.AreEqual(1, result.Where(items => items.Item1 == TelegramCommand.RemoveAdminPermission).Count());
        }

        [TestMethod]
        public async Task TestGenerateMenuCommandsAsyncNoAdmin()
        {
            BotDbContext db = Utils.CreateMemoryDbContext();
            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            TelegramCommandGenerator telegramCommand = new TelegramCommandGenerator(telegramHandler);

            var result = await telegramCommand.GenerateMenuCommandsAsync(123456789);
            Assert.AreEqual(1, result.Where(items => items.Item1 == TelegramCommand.AddAdminPermission).Count());
        }

        [TestMethod]
        public async Task TestGenerateBindCommandsAsyncAppAuth()
        {
            await Utils.SetDefaultValueDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();
            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            TelegramCommandGenerator telegramCommand = new TelegramCommandGenerator(telegramHandler);

            var result = await telegramCommand.GenerateBindCommandsAsync(123456789);
            Assert.AreEqual(7, result.Count());
        }

        [TestMethod]
        public async Task TestGenerateBindCommandsAsyncApp()
        {
            Guid clientId = Guid.NewGuid();
            await Utils.SetOneValueDbContextAsync(clientId);
            BotDbContext db = Utils.CreateMemoryDbContext();
            var auth = await db.AppAuths.FirstAsync();
            db.AppAuths.Remove(auth);
            await db.SaveChangesAsync();
            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            TelegramCommandGenerator telegramCommand = new TelegramCommandGenerator(telegramHandler);

            var result = await telegramCommand.GenerateBindCommandsAsync(123456789);
            Assert.AreEqual(4, result.Count());
        }

        [TestMethod]
        public async Task TestGenerateBindCommandsAsync()
        {
            BotDbContext db = Utils.CreateMemoryDbContext();
            TelegramHandler telegramHandler = new TelegramHandler(logger, configuration, db);
            TelegramCommandGenerator telegramCommand = new TelegramCommandGenerator(telegramHandler);

            var result = await telegramCommand.GenerateBindCommandsAsync(123456789);
            Assert.AreEqual(1, result.Count());
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await Utils.DeleteDBAsync();
        }
    }
}
