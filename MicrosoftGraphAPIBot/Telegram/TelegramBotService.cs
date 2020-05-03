using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.Telegram
{
    /// <summary>
    /// Telegram Bot 服務
    /// </summary>
    internal class TelegramBotService : IHostedService
    {
        private readonly ILogger logger;
        private readonly TelegramHandler telegramHandler;

        public TelegramBotService(ILogger<TelegramBotService> logger, TelegramHandler telegramHandler)
        {
            this.logger = logger;
            this.telegramHandler = telegramHandler;
        }

        /// <summary>
        /// 啟動服務
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            telegramHandler.StartReceiving();
            logger.LogInformation("Telegram bot service is starting.");
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// 停止服務
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            telegramHandler.StopReceiving();
            logger.LogInformation("Telegram bot service is stopping.");

            return Task.CompletedTask;
        }
    }
}
