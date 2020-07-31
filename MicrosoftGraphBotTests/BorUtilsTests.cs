using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class BorUtilsTests
    {
        [TestMethod]
        public void TestCheckConfig()
        {
            Dictionary<string, string> config = new Dictionary<string, string>
            {
                { "JoinBotMessage", string.Empty },
                { "Cron", string.Empty },
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
    }
}
