using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.Telegram;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MicrosoftGraphAPIBot.Services
{
    /// <summary>
    /// Telegram Bot 服務
    /// </summary>
    public class TelegramBotService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly ITelegramBotClient botClient;
        private bool isStart = false;

        /// <summary>
        /// Create a new TelegramBotService instance.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="botClient"></param>
        public TelegramBotService(ILogger<TelegramBotService> logger, IServiceProvider serviceProvider, ITelegramBotClient botClient) =>
            (this.logger, this.serviceProvider, this.botClient) = (logger, serviceProvider, botClient);

        /// <summary>
        /// 啟動服務
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartReceiving();
            isStart = true;
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
            if (isStart)
                StopReceiving();
            logger.LogInformation("Telegram bot service is stopping.");

            return Task.CompletedTask;
        }

        /// <summary>
        /// 開始接收 Bot 的訊息
        /// </summary>
        private void StartReceiving()
        {
            botClient.OnMessage += BotOnMessageReceived;
            botClient.OnMessageEdited += BotOnMessageReceived;
            botClient.OnCallbackQuery += BotOnCallbackQuery;
            botClient.StartReceiving(Array.Empty<UpdateType>());

            logger.LogInformation("開始接收 Bot 的訊息");
        }

        /// <summary>
        /// 停止接收 Bot 的訊息
        /// </summary>
        private void StopReceiving()
        {
            botClient.StopReceiving();
            botClient.OnMessage -= BotOnMessageReceived;
            botClient.OnMessageEdited -= BotOnMessageReceived;
            botClient.OnCallbackQuery -= BotOnCallbackQuery;

            logger.LogInformation("停止接收 Bot 的訊息");
        }

        /// <summary>
        /// 接收來自 Telegram 的訊息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="messageEventArgs"></param>
        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            Message message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            IServiceProvider scopeServiceProvider = scope.ServiceProvider;
            TelegramHandler telegramHandler = scopeServiceProvider.GetService(typeof(TelegramHandler)) as TelegramHandler;
            await telegramHandler.MessageReceivedHandler(message);
        }

        /// <summary>
        /// 接收使用者透過 Telegram 回傳的 Query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="messageEventArgs"></param>
        private async void BotOnCallbackQuery(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            CallbackQuery callbackQuery = callbackQueryEventArgs.CallbackQuery;
            if (callbackQuery == null)
                return;

            using IServiceScope scope = this.serviceProvider.CreateScope();
            IServiceProvider scopeServiceProvider = scope.ServiceProvider;
            TelegramHandler telegramHandler = scopeServiceProvider.GetService(typeof(TelegramHandler)) as TelegramHandler;
            await telegramHandler.CallbackQueryHandler(callbackQuery);
        }
    }
}
