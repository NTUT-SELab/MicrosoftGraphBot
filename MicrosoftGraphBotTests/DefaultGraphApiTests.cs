using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using MicrosoftGraphAPIBot.Models;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    [TestClass]
    public class DefaultGraphApiTests
    {
        [TestMethod]
        public async Task TestIsValidApplicationAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "ValidApplicationResult.json");
            string json = File.ReadAllText(testResultPath);
            Guid clientId = Guid.NewGuid();
            var mocks = CreateGetTokenAsyncMock(json, clientId);

            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(mocks.Item1, mocks.Item2, mocks.Item3);
            Assert.IsTrue(await defaultGraphApi.IsValidApplicationAsync("test@onmicrosoft.com", string.Empty, string.Empty));
        }

        [TestMethod]
        public async Task TestIsInvalidApplicationAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "InvalidApplicationResult.json");
            string json = File.ReadAllText(testResultPath);
            Guid clientId = Guid.NewGuid();
            var mocks = CreateGetTokenAsyncMock(json, clientId);

            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(mocks.Item1, mocks.Item2, mocks.Item3);
            Assert.IsFalse(await defaultGraphApi.IsValidApplicationAsync("test@onmicrosoft.com", string.Empty, string.Empty));
        }

        [TestMethod]
        public async Task TestGetTokenSuccessAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "GetTokenSuccessResult.json");
            string json = File.ReadAllText(testResultPath);
            Guid clientId = Guid.NewGuid();
            var mocks = CreateGetTokenAsyncMock(json, clientId);

            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(mocks.Item1, mocks.Item2, mocks.Item3);
            (string, string) tokens = await defaultGraphApi.GetTokenAsync(clientId, string.Empty);

            JObject jObject = JObject.Parse(json);
            Assert.AreEqual(jObject["access_token"].ToString(), tokens.Item1);
            Assert.AreEqual(jObject["refresh_token"].ToString(), tokens.Item2);
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task TestGetTokenFailedAsync()
        {
            string testResultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiResults", "GetTokenFailedResult.json");
            string json = File.ReadAllText(testResultPath);
            Guid clientId = Guid.NewGuid();
            var mocks = CreateGetTokenAsyncMock(json, clientId);

            DefaultGraphApi defaultGraphApi = new DefaultGraphApi(mocks.Item1, mocks.Item2, mocks.Item3);
            (string, string) _ = await defaultGraphApi.GetTokenAsync(clientId, string.Empty);
        }

        [TestMethod]
        public async Task TeskGetUserInfoSuccessAsync()
        {
            string token = await Utils.GetTestToken();
            Assert.IsNotNull(await DefaultGraphApi.GetUserInfoAsync(token));
        }

        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod]
        public async Task TeskGetUserInfoFailedAsync()
        {
            _ = await DefaultGraphApi.GetUserInfoAsync(string.Empty);
        }

        private (BotDbContext, ILogger<DefaultGraphApi>, IHttpClientFactory) CreateGetTokenAsyncMock(string json, Guid clientId)
        {
            var loggerMock = new Mock<ILogger<DefaultGraphApi>>();

            //  Mock HttpClientFactory
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json),
            };
            handlerMock.Protected()
               .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
               .ReturnsAsync(response);
            var httpClient = new HttpClient(handlerMock.Object);
            var clientFactoryMock = new Mock<IHttpClientFactory>();
            clientFactoryMock.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Mock DbContext
            var azureApps = new List<AzureApp>
            {
                new AzureApp { Id = clientId, Email = "test@onmicrosoft.com", Secrets = string.Empty }
            }.AsQueryable();
            var mockDbSet = azureApps.BuildMockDbSet();
            mockDbSet.Setup(x => x.FindAsync(clientId)).ReturnsAsync((object[] ids) =>
            {
                var id = (Guid)ids[0];
                return azureApps.FirstOrDefault(x => x.Id == id);
            });
            var mockDbContext = new Mock<BotDbContext>();
            mockDbContext.Setup(c => c.AzureApps).Returns(mockDbSet.Object);

            return (mockDbContext.Object, loggerMock.Object, clientFactoryMock.Object);
        }
    }
}
