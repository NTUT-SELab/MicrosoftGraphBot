using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using MicrosoftGraphAPIBot.Models;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MicrosoftGraphBotTests
{
    public static class Utils
    {
        private static string accessToken = string.Empty;

        public static async Task<string> GetTestToken()
        {
            if (accessToken == string.Empty)
            {
                Uri tokenUrl = new Uri("https://raw.githubusercontent.com/NTUT-SELab/MicrosoftGraphToken/master/Token.txt");

                using HttpClient httpClient = new HttpClient();
                string json = await httpClient.GetStringAsync(tokenUrl);
                accessToken = JObject.Parse(json)["access_token"].ToString();
            }

            return accessToken;
        }

        public static (ILogger<DefaultGraphApi>, IHttpClientFactory) CreateDefaultGraphApiMock(string json)
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

            return (loggerMock.Object, clientFactoryMock.Object);
        }

        public static BotDbContext CreateMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<BotDbContext>()
                  .UseInMemoryDatabase("TestDb")
                  .Options;
            BotDbContext db = new BotDbContext(options);

            return db;
        }

        public static async Task SetOneValueDbContextAsync(Guid clientId)
        {
            using BotDbContext db = CreateMemoryDbContext();

            db.TelegramUsers.Add(new TelegramUser { Id = 123456789, UserName = "Test Bot" });
            db.AzureApps.Add(new AzureApp { Id = clientId, Email = "test@onmicrosoft.com", Secrets = string.Empty, TelegramUserId = 123456789 });
            db.AppAuths.Add(new AppAuth { Name = "test", RefreshToken = string.Empty, Scope = DefaultGraphApi.Scope, AzureAppId = clientId });

            await db.SaveChangesAsync();
        }

        public static async Task SetDefaultValueDbContextAsync()
        {
            using BotDbContext db = CreateMemoryDbContext();

            db.TelegramUsers.Add(new TelegramUser { Id = 123456789, UserName = "Test Bot" });
            db.TelegramUsers.Add(new TelegramUser { Id = 987654321, UserName = "Test Bot" });
            Guid clientId1 = Guid.NewGuid();
            Guid clientId2 = Guid.NewGuid();
            Guid clientId3 = Guid.NewGuid();
            db.AzureApps.Add(new AzureApp { Id = clientId1, Email = "test@onmicrosoft.com", Secrets = string.Empty, TelegramUserId = 123456789, Name = "App1" });
            db.AzureApps.Add(new AzureApp { Id = clientId2, Email = "test5@onmicrosoft.com", Secrets = string.Empty, TelegramUserId = 123456789, Name = "App2" });
            db.AzureApps.Add(new AzureApp { Id = clientId3, Email = "test1@onmicrosoft.com", Secrets = string.Empty, TelegramUserId = 987654321, Name = "App3" });
            db.AppAuths.Add(new AppAuth { Name = "Auth1", RefreshToken = string.Empty, Scope = DefaultGraphApi.Scope, AzureAppId = clientId1 });
            db.AppAuths.Add(new AppAuth { Name = "Auth3", RefreshToken = string.Empty, Scope = "", AzureAppId = clientId3 });

            await db.SaveChangesAsync();
            await db.DisposeAsync();
        }

        public static async Task SetApiResultDbContextAsync()
        {
            using BotDbContext db = CreateMemoryDbContext();

            db.TelegramUsers.Add(new TelegramUser { Id = 123456789, UserName = "Test Bot" });
            db.TelegramUsers.Add(new TelegramUser { Id = 987654321, UserName = "Test Bot" });

            db.ApiResults.Add(new ApiResult { Result = "123", TelegramUserId = 123456789 });
            db.ApiResults.Add(new ApiResult { Result = "123987456", TelegramUserId = 123456789 });
            db.ApiResults.Add(new ApiResult { Result = "987654321", TelegramUserId = 987654321 });

            await db.SaveChangesAsync();
            await db.DisposeAsync();
        }

        public static async Task DeleteDBAsync()
        {
            BotDbContext db = CreateMemoryDbContext();
            await db.Database.EnsureDeletedAsync();
        }
    }
}
