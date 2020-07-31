using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
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

        #region Config

        private static readonly string[] configKeys = new string[]
        {
            "JoinBotMessage",
            "Cron",
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
        public BotException(string? message) : base(message) { }

        public BotException(string? message, Exception? innerException) : base(message, innerException) { }

        protected BotException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#nullable disable
    }
}
