using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class ApiControllerTests
    {
        private readonly IGraphServiceClient graphClient;
        private readonly ILogger<ApiController> logger;

        public ApiControllerTests()
        {
            string token = Utils.GetTestToken().Result;
            graphClient = DefaultGraphApi.GetGraphServiceClient(token);
            var mock = new Mock<ILogger<ApiController>>();
            logger = mock.Object;
        }

        [TestMethod]
        public async Task TestApiRunNoConfig()
        {
            OutlookApi outlookApi = new OutlookApi(null);
            ApiController apiController = new ApiController(logger, null, new GraphApi[] { outlookApi });
            IEnumerable<(string, string, bool)> results = await apiController.RunAsync(graphClient, "Test");

            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task TestApiRunAll()
        {
            Dictionary<string, string> config = new Dictionary<string, string>
            {
                {"API:NumberOfMethodCall", "0"},
                {"API:NumberOfServiceCall", "0" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            OutlookApi outlookApi = new OutlookApi(null, configuration);
            ApiController apiController = new ApiController(logger, configuration, new GraphApi[] { outlookApi });
            IEnumerable<(string, string, bool)> results = await apiController.RunAsync(graphClient, "Test");

            Assert.AreEqual(3, results.Count());
        }

        [TestMethod]
        public async Task TestApiRun1()
        {
            Dictionary<string, string> config = new Dictionary<string, string>
            {
                {"API:NumberOfMethodCall", "1"},
                {"API:NumberOfServiceCall", "0" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            OutlookApi outlookApi = new OutlookApi(null, configuration);
            ApiController apiController = new ApiController(logger, configuration, new GraphApi[] { outlookApi });
            IEnumerable<(string, string, bool)> results = await apiController.RunAsync(graphClient, "Test");

            Assert.AreEqual(1, results.Count());
        }
    }
}
