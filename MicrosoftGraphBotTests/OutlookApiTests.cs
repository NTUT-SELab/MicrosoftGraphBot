using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class OutlookApiTests
    {
        private readonly IGraphServiceClient graphClient;

        public OutlookApiTests()
        {
            string token = Utils.GetTestToken().Result;
            graphClient = DefaultGraphApi.GetGraphServiceClient(token);
        }

        [TestMethod]
        public async Task TestCallCreateMessageAsync()
        {
            OutlookApi outlookApi = new OutlookApi(graphClient);
            bool result = await outlookApi.CallCreateMessageAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallUpdateMessage()
        {
            OutlookApi outlookApi = new OutlookApi(graphClient);
            bool result = await outlookApi.CallUpdateMessageAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestCallSendMessage()
        {
            OutlookApi outlookApi = new OutlookApi(graphClient);
            bool result = await outlookApi.CallSendMessageAsync();
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task TestApiRunNoConfig()
        {
            OutlookApi outlookApi = new OutlookApi(null);
            IEnumerable<(string, bool)> results = await outlookApi.RunAsync(graphClient).ToListAsync();

            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task TestApiRunAll()
        {
            Dictionary<string, string> config = new Dictionary<string, string>
            {
                {"API:NumberOfMethodCall", "0"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            OutlookApi outlookApi = new OutlookApi(null, configuration);
            IEnumerable<(string, bool)> results = await outlookApi.RunAsync(graphClient).ToListAsync();

            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task TestApiRun1()
        {
            Dictionary<string, string> config = new Dictionary<string, string>
            {
                {"API:NumberOfMethodCall", "1"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            OutlookApi outlookApi = new OutlookApi(null, configuration);
            IEnumerable<(string, bool)> results = await outlookApi.RunAsync(graphClient).ToListAsync();

            Assert.AreEqual(1, results.Count());
        }
    }
}
