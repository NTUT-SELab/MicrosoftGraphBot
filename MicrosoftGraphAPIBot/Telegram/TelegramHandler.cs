using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

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
        private readonly Dictionary<string, (string, Func<Message, Task>)> menu;

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

            menu = new Dictionary<string, (string, Func<Message, Task>)>
            {
                { "/help", ("指令選單", Help) },
                { "/bind", ("帳號綁定", RegisterApp) }
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

            logger.LogDebug("User Id: {0}", message.Chat.Id);

            string userMessage = message.Text.Split(' ').First();

            if (menu.ContainsKey(userMessage))
                await menu[userMessage].Item2.Invoke(message);
            else
                switch (userMessage)
                {
                    case "/start":
                        await Start(message);
                        break;
                    default:
                        await Defult(message);
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
            result.AddRange(menu.Select(dictionary => $"{dictionary.Key, -15} {dictionary.Value.Item1}"));

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.Join('\n', result)
            );
        }

        /// <summary>
        /// 註冊新的應用程式到 Azure
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task RegisterApp(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.Format("註冊應用程式: [Get an app ID and secret]({0})", BindHandler.AppRegistrationUrl),
                ParseMode.MarkdownV2,
                replyMarkup: new ForceReplyMarkup()
            );
        }

        /// <summary>
        /// 處理預設指令以外的事件
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task Defult(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.Format("Hi @{0} 請使用 /help 獲得完整指令", message.Chat.Username)
            );
        }
    }
}
