using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using MicrosoftGraphAPIBot.Models;
using MicrosoftGraphAPIBot.Telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.MicrosoftGraph
{
    /// <summary>
    /// 觸發 Api 介面
    /// </summary>
    public class ApiCallManager
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly DefaultGraphApi defaultGraphApi;

        public ApiCallManager(ILogger<ApiCallManager> logger, IServiceProvider serviceProvider, DefaultGraphApi defaultGraphApi) =>
            (this.logger, this.serviceProvider, this.defaultGraphApi) = (logger, serviceProvider, defaultGraphApi);

        /// <summary>
        /// 提供排程觸發 Api
        /// </summary>
        /// <returns></returns>
        public async Task RunAsync()
        {
            logger.LogInformation("開始執行 Api 任務(所有使用者)");
            BotDbContext db = serviceProvider.GetService(typeof(BotDbContext)) as BotDbContext;
            List<long> usersId = await db.TelegramUsers.AsQueryable().Select(user => user.Id).ToListAsync();
            IEnumerable<Task<(long, string)>> callApiTasks = usersId.Select(userId => RunAsync(userId, true));
            IEnumerable<(long, string)> callApiResults = await Task.WhenAll(callApiTasks);

            TelegramController telegramHandler = serviceProvider.GetService(typeof(TelegramController)) as TelegramController;
            if (callApiResults.Any())
            {
                IEnumerable<Task> sendMessagesTask = callApiResults.Select(items => telegramHandler.SendMessage(items.Item1, !string.IsNullOrEmpty(items.Item2) ? items.Item2 : "沒有任何可執行物件"));
                Task task = Task.WhenAll(sendMessagesTask);
                await task;
            }
        }

        /// <summary>
        /// 使用者手動觸發 Api
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <param name="newRequest"> 是否用新的請求執行 </param>
        /// <returns> (Telegram user id, call api result message) </returns>
        public async Task<(long, string)> RunAsync(long userId, bool newRequest = false)
        {
            if (newRequest)
            {
                using IServiceScope scope = this.serviceProvider.CreateScope();
                return await RunAsync(userId, scope.ServiceProvider);
            }
            else
                return await RunAsync(userId, this.serviceProvider);
        }

        /// <summary>
        /// 使用者手動觸發 Api
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <param name="serviceProvider"></param>
        /// <returns> (Telegram user id, call api result message) </returns>
        private async Task<(long, string)> RunAsync(long userId, IServiceProvider serviceProvider)
        {
            try
            {
                BotDbContext db = serviceProvider.GetService(typeof(BotDbContext)) as BotDbContext;
                List<AppAuth> appAuths = await db.AppAuths.Include(appAuth => appAuth.AzureApp).Where(auth => auth.AzureApp.TelegramUser.Id == userId).ToListAsync();

                IEnumerable<AppAuth> invalidScopeAuths = appAuths.Where(auth => auth.Scope != DefaultGraphApi.Scope);
                IEnumerable<AppAuth> validScopeAuths = appAuths.Where(auth => auth.Scope == DefaultGraphApi.Scope);

                if (invalidScopeAuths.Any())
                {
                    List<string> messages = new List<string> { "無效授權:" };
                    messages.AddRange(invalidScopeAuths.Select(auth => auth.Name));
                    TelegramController telegramHandler = serviceProvider.GetService(typeof(TelegramController)) as TelegramController;
                    await telegramHandler.SendMessage(userId, string.Join('\n', messages));
                    await telegramHandler.ReBindAuth(userId);
                }

                List<string> callApiResults = new List<string>();

                if (validScopeAuths.Any())
                {
                    IEnumerable<Task<(string, string)>> accessTokenTasks = appAuths.Select(appAuth => defaultGraphApi.ReflashTokenAsync(appAuth));
                    IEnumerable<(string, string)> accessTokens = await Task.WhenAll(accessTokenTasks);
                    db.AppAuths.UpdateRange(appAuths);
                    Task<int> saveChangeTask = db.SaveChangesAsync();

                    IEnumerable<Task<string>> callApiTasks = accessTokens.Select(accessToken => CallApiAsync(accessToken.Item1, accessToken.Item2));
                    callApiResults.AddRange(await Task.WhenAll(callApiTasks));

                    await saveChangeTask;
                }

                logger.LogInformation($"userId: {userId}, Api呼叫執行完成");
                return (userId, string.Join('\n', callApiResults));
            }
            catch (BotException ex)
            {
                logger.LogError($"userId: {userId}, ErrorMessage: {ex.Message}");
                return (userId, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError($"userId: {userId}, ErrorMessage: {ex.Message}");
                return (userId, "發生異常錯誤，請聯絡系統管理員調取日誌訊息");
            }
        }

        /// <summary>
        /// 要求 ApiController 開始執行 Microsoft Graph 服務
        /// </summary>
        /// <param name="token"> The requested access token. 
        /// App can use this token to call Microsoft Graph. </param>
        /// <param name="authName"> token 別名 </param>
        /// <returns> call api result message </returns>
        private async Task<string> CallApiAsync(string token, string authName)
        {
            try
            {
                IGraphServiceClient graphClient = DefaultGraphApi.GetGraphServiceClient(token);
                ApiController apiController = serviceProvider.GetService(typeof(ApiController)) as ApiController;
                var results = await apiController.RunAsync(graphClient, authName);
                IEnumerable<string> resultMessages = results.Select(result => string.Format("授權名稱: {0}, 服務: {1}, Api模組: {2}, 結果: {3}", authName, result.Item1, result.Item2, result.Item3 ? "Success" : "Fail"));
                foreach (string resultMessage in resultMessages)
                    logger.LogInformation(resultMessage);
                return string.Join('\n', resultMessages);
            }
            catch
            {
                string errorMessage = $"授權名稱: {authName}, 所有服務呼叫: Fail";
                logger.LogError(errorMessage);
                return errorMessage;
            }
        }
    }
}
