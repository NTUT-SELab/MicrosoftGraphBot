using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.Models;
using MicrosoftGraphAPIBot.Telegram;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot
{
    public static class Utils
    {
        /// <summary>
        /// 將 appsettings.json 中的 SQL 參數合併成連接字串
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static string GetDBConnection(IConfiguration configuration)
        {
            string SQLHost = configuration["MSSQL:Host"];
            string SQLPort = configuration["MSSQL:Port"];
            string SQLUser = configuration["MSSQL:User"];
            string SQLPassword = configuration["MSSQL:Password"];
            string SQLDataBase = configuration["MSSQL:DataBase"];
            return string.Format("Data Source={0},{1};Initial Catalog={2};User ID={3};Password={4}", SQLHost, SQLPort, SQLDataBase, SQLUser, SQLPassword);
        }

        /// <summary>
        /// 檢查是否有新版本，並通知管理員
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static async Task CheckAppVersionAsync(IServiceProvider serviceProvider)
        {
            bool needUpdate = await CheckNeedUpdateAsync(serviceProvider);

            if (needUpdate)
            {
                BotDbContext db = serviceProvider.GetService(typeof(BotDbContext)) as BotDbContext;
                List<long> usersIds = await db.TelegramUsers.AsQueryable().Where(user => user.IsAdmin).Select(user => user.Id).ToListAsync();

                TelegramController telegramHandler = serviceProvider.GetService(typeof(TelegramController)) as TelegramController;
                IEnumerable<Task> sendMessageTasks = usersIds.Select(userId => telegramHandler.SendMessage(userId, "Bot 有新版本需要更新 \n https://github.com/NTUT-SELab/MicrosoftGraphBot"));
                await Task.WhenAll(sendMessageTasks);
            }
        }

        /// <summary>
        /// 推播呼叫 Api 的結果給使用者
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static async Task PushApiResultAsync(IServiceProvider serviceProvider)
        {
            BotDbContext db = serviceProvider.GetService(typeof(BotDbContext)) as BotDbContext;
            IList<ApiResult> apiResults = await db.ApiResults.Include(result => result.TelegramUser).ToListAsync();
            if (apiResults.Any())
            {
                IEnumerable<long> usersIds = apiResults.Select(item => item.TelegramUserId).Distinct();

                TelegramController telegramHandler = serviceProvider.GetService(typeof(TelegramController)) as TelegramController;
                IEnumerable<Task> sendMessageTasks = usersIds.Select(usersId => telegramHandler.SendMessage(usersId,
                    string.Join('\n', apiResults.Where(item => item.TelegramUserId == usersId).OrderBy(item => item.Date).Select(item => $"時間: {item.Date}, 結果:\n{item.Result}"))));
                db.ApiResults.RemoveRange(apiResults);

                Task sendMessageTask = Task.WhenAll(sendMessageTasks);
                Task dbTask = db.SaveChangesAsync();

                await sendMessageTask;
                await dbTask;
            }
        }

        /// <summary>
        /// 檢查是否有新版本
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        private static async Task<bool> CheckNeedUpdateAsync(IServiceProvider serviceProvider)
        {
            ILogger logger = serviceProvider.GetService(typeof(ILogger<Program>)) as ILogger<Program>;
            
            try
            {
                Version localVer = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString());

                IHttpClientFactory clientFactory = serviceProvider.GetService(typeof(IHttpClientFactory)) as IHttpClientFactory;
                HttpClient httpClient = clientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "http://developer.github.com/v3/#user-agent-required");
                string githubJson = await httpClient.GetStringAsync("https://api.github.com/repos/NTUT-SELab/MicrosoftGraphBot/releases/latest");

                JObject jObject = JObject.Parse(githubJson);
                Version remoteVer = new Version(jObject["tag_name"].ToString());

                logger.LogDebug($"localVer: {localVer}");
                logger.LogDebug($"remoteVer: {remoteVer}");

                if (remoteVer > localVer)
                    return true;
                return false;
            }
            catch(Exception ex)
            {
                logger.LogError($"CheckNeedUpdate: {ex.Message}");
                return false;
            }
        }

#region Config

        private static readonly string[] configKeys = new string[]
        {
            "JoinBotMessage",
            "Cron",
            "CheckVerCron",
            "PushResultCron",
            "AdminPassword",
            "Telegram:Token",
            "MSSQL:Host",
            "MSSQL:Port",
            "MSSQL:User",
            "MSSQL:Password",
            "MSSQL:DataBase",
            "API:NumberOfServiceCall",
            "API:NumberOfMethodCall"
        };

        /// <summary>
        /// 檢查 appsettings.json 中的必要參數是否存在
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static bool CheckConfig(IConfiguration configuration)
        {
            bool isExist = true;
            object isExistLock = new object();

            Parallel.ForEach(configKeys, (configKey) => {
                if (!configuration.AsEnumerable().Any(item => item.Key == configKey))
                    lock (isExistLock)
                        isExist = false;
            });

            return isExist;
        }

#endregion
    }

    [Serializable]
    public class BotException : Exception
    {
#nullable enable
        public BotException() : base("Bot exception.") { }
        public BotException(string? message) : base(message) { }

        public BotException(string? message, Exception? innerException) : base(message, innerException) { }

        protected BotException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#nullable disable
    }
}
