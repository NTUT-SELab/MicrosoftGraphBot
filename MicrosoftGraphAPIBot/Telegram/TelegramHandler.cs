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

namespace MicrosoftGraphAPIBot.Telegram
{
    /// <summary>
    /// 處理 Telegram Bot 相關行為
    /// </summary>
    public partial class TelegramHandler
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly ITelegramBotClient botClient;
        private readonly BindHandler bindHandler;
        private readonly TelegramCommandGenerator commandGenerator;
        private readonly Dictionary<string, (Func<Message, Task>, Func<Message, Task>)> Controller;

        /// <summary>
        /// Create a new TelegramHandler instance.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public TelegramHandler(ILogger<TelegramHandler> logger, IConfiguration configuration, ITelegramBotClient botClient, BindHandler bindHandler, TelegramCommandGenerator commandGenerator)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.botClient = botClient;
            this.bindHandler = bindHandler;
            this.commandGenerator = commandGenerator;

            // key = 指令, value = (指令對應的方法, 使用者回復指令訊息對應的方法)
            Controller = new Dictionary<string, (Func<Message, Task>, Func<Message, Task>)>
            {
                { TelegramCommand.Start, (Start, null)},
                { TelegramCommand.Help, (Help, null) },
                { TelegramCommand.Bind, (Bind, null) },
                { TelegramCommand.RegApp, (RegisterApp, ReplayRegisterApp) },
                { TelegramCommand.DeleteApp, (DeleteApp, null)},
                { TelegramCommand.QueryApp, (QueryApp, null) },
                { TelegramCommand.BindAuth, (BindUserAuth, null) },
                { TelegramCommand.UnbindAuth, (UnbindUserAuth, null) },
                { TelegramCommand.QueryAuth, (QueryUserAuth, null) }
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
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            if (message.ReplyToMessage != null && message.ReplyToMessage.From.Id == botClient.BotId)
            {
                string replyCommand = message.ReplyToMessage.Text.Split('\n').First();
                await Controller[replyCommand].Item2.Invoke(message).ConfigureAwait(false);
                return;
            }

            string command = message.Text.Split(' ').First();

            if (Controller.ContainsKey(command))
            {
                await Controller[command].Item1.Invoke(message).ConfigureAwait(false);
                return;
            }
                
            await Defult(message).ConfigureAwait(false);
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
                text: configuration["JoinBotMessage"]
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
            List<string> result = new List<string> { "指令選單:", ""};
            IEnumerable<(string, string)> menu = commandGenerator.GenerateMenuCommands();
            result.AddRange(menu.Select(command => $"{command.Item1, -15} {command.Item2}"));

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.Join('\n', result)
            );
        }

        /// <summary>
        /// 處理 /bind 事件
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task Bind(Message message)
        {
            List<string> result = new List<string> { "綁定指令選單:", "" };
            IEnumerable<(string, string)> menu = await commandGenerator.GenerateBindCommandsAsync(message.Chat.Id);
            result.AddRange(menu.Select(command => $"{command.Item1,-15} {command.Item2}"));

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.Join('\n', result)
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
