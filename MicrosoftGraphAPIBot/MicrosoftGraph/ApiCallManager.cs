using Microsoft.EntityFrameworkCore;
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
        private readonly BotDbContext db;
        private readonly DefaultGraphApi defaultGraphApi;

        public ApiCallManager(ILogger<ApiCallManager> logger, IServiceProvider serviceProvider, BotDbContext botDbContext, DefaultGraphApi defaultGraphApi) =>
            (this.logger, this.serviceProvider, this.db, this.defaultGraphApi) = (logger, serviceProvider, botDbContext, defaultGraphApi);

        /// <summary>
        /// 提供排程觸發 Api
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            logger.LogInformation("排程執行呼叫Api開始");
            List<long> usersId = await db.TelegramUsers.AsQueryable().Select(user => user.Id).ToListAsync();
            IEnumerable<Task<(long, string)>> callApiTasks = usersId.Select(userId => Run(userId));
            IEnumerable<(long, string)> callApiResults = await Task.WhenAll(callApiTasks);

            TelegramHandler telegramHandler = serviceProvider.GetService(typeof(TelegramHandler)) as TelegramHandler;
            IEnumerable<Task> sendMessagesTask = callApiResults.Select(items => telegramHandler.SendMessage(items.Item1, items.Item2));
            Task task = Task.WhenAll(sendMessagesTask);
            task.Wait();
        }

        /// <summary>
        /// 使用者手動觸發 Api
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <returns> (Telegram user id, call api result message) </returns>
        public async Task<(long, string)> Run(long userId)
        {
            try
            {
                List<AppAuth> appAuths = await db.AppAuths.Include(appAuth => appAuth.AzureApp).Where(auth => auth.AzureApp.TelegramUser.Id == userId).ToListAsync();
                IEnumerable<Task<(string, string)>> accessTokenTasks = appAuths.Select(appAuth => defaultGraphApi.ReflashTokenAsync(appAuth));
                IEnumerable<(string, string)> accessTokens = await Task.WhenAll(accessTokenTasks);
                db.AppAuths.UpdateRange(appAuths);
                Task<int> saveChangeTask = db.SaveChangesAsync();

                IEnumerable<Task<string>> callApiTasks = accessTokens.Select(accessToken => CallApi(accessToken.Item1, accessToken.Item2));
                IEnumerable<string> callApiResults = await Task.WhenAll(callApiTasks);

                await saveChangeTask;
                logger.LogInformation($"user: {userId}, Api呼叫執行完成");
                return (userId, string.Join('\n', callApiResults));
            }
            catch(InvalidOperationException ex)
            {
                logger.LogError($"userId: {userId}, ErrorMessage: {ex.Message}");
                return (userId, ex.Message);
            }
            catch(Exception ex)
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
        private async Task<string> CallApi(string token, string authName)
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
