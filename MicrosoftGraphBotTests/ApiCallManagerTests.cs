using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using MicrosoftGraphAPIBot.Telegram;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class ApiCallManagerTests
    {
        private readonly ServiceCollection services;

        public ApiCallManagerTests()
        {
            TelegramController telegramHandler = new Mock<TelegramController>(null, null, null, null, null, null, null).Object;

            services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped<GraphApi, OutlookApi>();
            services.AddScoped<ApiController>();
            services.AddScoped<ApiCallManager>(); 
            services.AddScoped<DefaultGraphApi>();
            services.AddScoped(services => telegramHandler);
            services.AddScoped(services => Utils.CreateMemoryDbContext());

            Dictionary<string, string> config = new Dictionary<string, string>
            {
                {"API:NumberOfMethodCall", "2"},
                {"API:NumberOfServiceCall", "1" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(config)
                .Build();

            services.AddScoped<IConfiguration>(service => configuration);
        }

        [TestMethod]
        public async Task TestRunOneUserValidToken()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "GetTokenSuccessResult.json");
            string json = File.ReadAllText(testResultPath);
            JObject jObject = JObject.Parse(json);
            jObject["access_token"] = await Utils.GetTestToken();
            json = JsonConvert.SerializeObject(jObject);
            var mocks = Utils.CreateDefaultGraphApiMock(json);
            var telegramMock = new Mock<TelegramController>(null, null, null, null, null, null, null).Object;
            await Utils.SetDefaultValueDbContextAsync();
            services.AddScoped(service => mocks.Item2);
            services.AddScoped(service => telegramMock);

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            ApiCallManager apiCallManager = serviceProvider.GetRequiredService<ApiCallManager>();
            (long, string) result = await apiCallManager.RunAsync(123456789, true);
            string[] message = result.Item2.Split('\n');

            Assert.AreEqual(2, message.Length);

            result = await apiCallManager.RunAsync(987654321, true);
            message = result.Item2.Split('\n');

            Assert.AreEqual(1, message.Length);
        }

        [TestMethod]
        public async Task TestRunOneUserInvalidToken()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "GetTokenFailedResult.json");
            string json = File.ReadAllText(testResultPath);
            var mocks = Utils.CreateDefaultGraphApiMock(json);
            await Utils.SetDefaultValueDbContextAsync();
            services.AddScoped(service => mocks.Item2);

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            ApiCallManager apiCallManager = serviceProvider.GetRequiredService<ApiCallManager>();
            (long, string) result = await apiCallManager.RunAsync(123456789);
            string[] message = result.Item2.Split('\n');

            Assert.AreEqual(1, message.Length);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            await Utils.DeleteDBAsync();
        }
    }
}
