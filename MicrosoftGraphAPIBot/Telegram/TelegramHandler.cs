using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        private readonly BindHandler bindHandler;
        private readonly Dictionary<string, (string, Func<Message, Task>)> defaultMenu;

        /// <summary>
        /// Create a new TelegramHandler instance.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public TelegramHandler(ILogger<TelegramHandler> logger, IConfiguration configuration, ITelegramBotClient botClient, BindHandler bindHandler)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.botClient = botClient;
            this.bindHandler = bindHandler;

            defaultMenu = new Dictionary<string, (string, Func<Message, Task>)>
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
        /// 處理來自 Bot 的訊息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="messageEventArgs"></param>
        private async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            Message message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            logger.LogDebug("User Id: {0}", message.Chat.Id);

            if (message.ReplyToMessage != null)
            {
                string[] userMessages = message.Text.Split(' ');
                await bindHandler.RegAppAsync(message, userMessages[0], userMessages[1], userMessages[2]);

                await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "應用程式註冊成功"
                );

                return;
            }

            string userMessage = message.Text.Split(' ').First();

            if (defaultMenu.ContainsKey(userMessage))
                await defaultMenu[userMessage].Item2.Invoke(message);
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
            result.AddRange(defaultMenu.Select(dictionary => $"{dictionary.Key, -15} {dictionary.Value.Item1}"));

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
                ParseMode.MarkdownV2
            );
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "/regApp" + "\n" + 
                    "[office365帳號] [Application (client) ID] [Client secrets]" + "\n" +
                    "AAA@BBB.onmicrosoft.com 9a448485-16dd-49c3-b4be-d8b7e138db27 lyfJ7f4k=9:qA?e:huHchb0pcBhMuk@b]",
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
