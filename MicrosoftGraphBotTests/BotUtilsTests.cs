using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot;
using MicrosoftGraphAPIBot.Models;
using MicrosoftGraphAPIBot.Telegram;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class BotUtilsTests
    {
        private readonly ServiceCollection services;

        public BotUtilsTests()
        {
            var telegramMock = new Mock<TelegramController>(null, null, null, null, null, null, null, null);
            telegramMock.Setup(m => m.SendMessage(It.IsAny<long>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            TelegramController telegramHandler = telegramMock.Object;

            services = new ServiceCollection();
            services.AddScoped(services => telegramHandler);
            services.AddScoped(services => Utils.CreateMemoryDbContext());
        }

        [TestMethod]
        public void TestCheckConfig()
        {
            Dictionary<string, string> config = new Dictionary<string, string>
            {
                { "JoinBotMessage", string.Empty },
                { "Cron", string.Empty },
                { "CheckVerCron", string.Empty },
                { "PushResultCron", string.Empty },
                { "AdminPassword", string.Empty },
                { "Telegram:Token", string.Empty },
                { "MSSQL:Host", string.Empty },
                { "MSSQL:Port", string.Empty },
                { "MSSQL:User", string.Empty },
                { "MSSQL:Password", string.Empty },
                { "MSSQL:DataBase", string.Empty },
                { "API:NumberOfServiceCall", string.Empty },
                { "API:NumberOfMethodCall", string.Empty }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            Assert.IsTrue(MicrosoftGraphAPIBot.Utils.CheckConfig(configuration));
        }

        [TestMethod]
        public void TestCheckConfigNoAnyData()
        {
            Dictionary<string, string> config = new Dictionary<string, string>();

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            Assert.IsFalse(MicrosoftGraphAPIBot.Utils.CheckConfig(configuration));
        }

        [TestMethod]
        public void TestTestCheckConfigSomeDataMissing()
        {
            Dictionary<string, string> config = new Dictionary<string, string>
            {
                { "JoinBotMessage", string.Empty },
                { "Cron", string.Empty },
                { "AdminPassword", string.Empty },
                { "Telegram:Token", string.Empty },
                { "MSSQL:Host", string.Empty },
                { "MSSQL:User", string.Empty },
                { "MSSQL:Password", string.Empty },
                { "MSSQL:DataBase", string.Empty },
                { "API:NumberOfServiceCall", string.Empty },
                { "API:NumberOfMethodCall", string.Empty }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            Assert.IsFalse(MicrosoftGraphAPIBot.Utils.CheckConfig(configuration));
        }

        [TestMethod]
        public async Task TestPushApiResultAsync()
        {
            await Utils.SetApiResultDbContextAsync();
            BotDbContext db = Utils.CreateMemoryDbContext();

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            await MicrosoftGraphAPIBot.Utils.PushApiResultAsync(serviceProvider);
            Assert.IsFalse(await db.ApiResults.AnyAsync());
        }

        [TestMethod]
        public async Task TestPushNoApiResultAsync()
        {
            await Utils.SetDefaultValueDbContextAsync();

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            await MicrosoftGraphAPIBot.Utils.PushApiResultAsync(serviceProvider);
        }

        [TestMethod]
        public void TestAssertTrue()
        {
            MicrosoftGraphAPIBot.Utils.Assert(true);
        }

        [TestMethod]
        [ExpectedException(typeof(BotException))]
        public void TestAssertFalse()
        {
            MicrosoftGraphAPIBot.Utils.Assert(false);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await Utils.DeleteDBAsync();
        }
    }
}
