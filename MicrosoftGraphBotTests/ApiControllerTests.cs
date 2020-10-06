using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class ApiControllerTests
    {
        private readonly IGraphServiceClient graphClient;
        private readonly ServiceCollection services;

        public ApiControllerTests()
        {
            string token = Utils.GetTestToken().Result;
            graphClient = DefaultGraphApi.GetGraphServiceClient(token);
            services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<GraphApi, OutlookApi>();
            services.AddScoped<GraphApi, OneDriveApi>();
            services.AddScoped<GraphApi, PermissionsApi>();
            services.AddScoped<GraphApi, CalendarApi>();
            services.AddScoped<GraphApi, PersonalContactsApi>();
            services.AddScoped<ApiController>();
        }

        [TestMethod]
        public async Task TestApiRunNoConfig()
        {
            Dictionary<string, string> config = new Dictionary<string, string>();

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            services.AddScoped<IConfiguration>(service => configuration);
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            ApiController apiController = serviceProvider.GetRequiredService<ApiController>();
            IEnumerable<(string, string, bool)> results = await apiController.RunAsync(graphClient, "Test");

            Assert.AreEqual(15, results.Count());
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

            services.AddScoped<IConfiguration>(service => configuration);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            
            ApiController apiController = serviceProvider.GetRequiredService<ApiController>();
            IEnumerable<(string, string, bool)> results = await apiController.RunAsync(graphClient, "Test");

            Assert.AreEqual(15, results.Count());
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

            services.AddScoped<IConfiguration>(service => configuration);
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            ApiController apiController = serviceProvider.GetRequiredService<ApiController>();
            IEnumerable<(string, string, bool)> results = await apiController.RunAsync(graphClient, "Test");

            Assert.AreEqual(5, results.Count());
        }
    }
}
