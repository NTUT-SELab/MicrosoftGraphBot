using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MicrosoftGraphAPIBot.Telegram
{
    /// <summary>
    /// 處理 Telegram Bot 相關行為
    /// </summary>
    public class TelegramHandler
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly ITelegramBotClient botClient;
        private readonly (string, string)[] menu;

        /// <summary>
        /// Create a new TelegramHandler instance.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public TelegramHandler(ILogger<TelegramHandler> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
            botClient = new TelegramBotClient(this.configuration["Telegram:Token"]);

            menu = new (string, string)[]
            {
                ("/help", "指令選單")
            };
        }

        /// <summary>
        /// 開始接收 Bot 的訊息
        /// </summary>
        public void StartReceiving()
        {
            botClient.OnMessage += BotOnMessageReceived;
            botClient.OnMessageEdited += BotOnMessageReceived;
            botClient.StartReceiving(Array.Empty<UpdateType>());

            logger.LogInformation("開始接收 Bot 的訊息");
        }

        /// <summary>
        /// 停止接收 Bot 的訊息
        /// </summary>
        public void StopReceiving()
        {
            botClient.StopReceiving();
            botClient.OnMessage -= BotOnMessageReceived;
            botClient.OnMessageEdited -= BotOnMessageReceived;
            logger.LogInformation("停止接收 Bot 的訊息");
        }

        /// <summary>
        /// 處理從 Bot 接收的訊息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="messageEventArgs"></param>
        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            switch (message.Text.Split(' ').First())
            {
                // Send inline keyboard
                case "/start":
                    await Start(message);
                    break;
                case "/help":
                    await Help(message);
                    break;
            }
        }

        /// <summary>
        /// 處理 /start 事件
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task Start(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "歡迎使用 Microsoft Graph Bot"
            );

            await Help(message);
        }

        /// <summary>
        /// 處理 /help 事件
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task Help(Message message)
        {
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            List<string> result = new List<string> { "指令選單:", ""};
            result.AddRange(menu.Select(value => $"{value.Item1, -15} {value.Item2}"));

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.Join('\n', result)
            );
        }
    }
}
