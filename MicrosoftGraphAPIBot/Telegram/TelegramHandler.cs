using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
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
        private readonly Dictionary<string, (Func<Message, Task>, Func<Message, Task>, Func<CallbackQuery, Task>)> Controller;

        /// <summary>
        /// Create a new TelegramHandler instance.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="botClient"></param>
        /// <param name="bindHandler"></param>
        /// <param name="commandGenerator"></param>
        public TelegramHandler(ILogger<TelegramHandler> logger, IConfiguration configuration, ITelegramBotClient botClient, BindHandler bindHandler, TelegramCommandGenerator commandGenerator)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.botClient = botClient;
            this.bindHandler = bindHandler;
            this.commandGenerator = commandGenerator;

            // key = 指令, value = (指令對應的方法, 使用者回復指令訊息對應的方法, 使用者回復選擇按鈕對應的方法)
            Controller = new Dictionary<string, (Func<Message, Task>, Func<Message, Task>, Func<CallbackQuery, Task>)>
            {
                { TelegramCommand.Start, (Start, null, null)},
                { TelegramCommand.Help, (Help, null, null) },
                { TelegramCommand.Bind, (Bind, null, null) },
                { TelegramCommand.RegApp, (RegisterApp, RegisterAppReplay, null) },
                { TelegramCommand.DeleteApp, (DeleteApp, null, null)},
                { TelegramCommand.QueryApp, (QueryApp, null, null) },
                { TelegramCommand.BindAuth, (BindUserAuth, BindUserAuthReplay, BindUserAuthCallback) },
                { TelegramCommand.UnbindAuth, (UnbindUserAuth, null, null) },
                { TelegramCommand.QueryAuth, (QueryUserAuth, null, null) }
            };
        }

        /// <summary>
        /// 處理來自 Bot 的訊息
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        public async Task MessageReceivedHandler(Message message)
        {
            if (message == null || message.Type != MessageType.Text)
                return;

            logger.LogDebug("User Id: {0}", message.Chat.Id);
            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            if (message.ReplyToMessage != null && message.ReplyToMessage.From.Id == botClient.BotId)
            {
                string replyCommand = message.ReplyToMessage.Text.Split('\n').First();
                await Controller[replyCommand].Item2.Invoke(message);
                return;
            }

            string command = message.Text.Split(' ').First();

            if (Controller.ContainsKey(command))
            {
                await Controller[command].Item1.Invoke(message);
                return;
            }
                
            await Defult(message).ConfigureAwait(false);
        }

        /// <summary>
        /// 處理來自 Bot 的訊息
        /// </summary>
        /// <param name="callbackQuery"> Telegram callbackQuery object </param>
        public async Task CallbackQueryHandler(CallbackQuery callbackQuery)
        {
            if (callbackQuery == null)
                return;

            await botClient.SendChatActionAsync(callbackQuery.From.Id, ChatAction.Typing);

            if (callbackQuery.Message != null && callbackQuery.Message.From.Id == botClient.BotId)
            {
                string callbackCommand = callbackQuery.Message.Text.Split('\n').First();
                await Controller[callbackCommand].Item3.Invoke(callbackQuery);
            }
        }

        /// <summary>
        /// 處理 /start 事件
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task Start(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: configuration["JoinBotMessage"]);

            await Help(message);
        }

        /// <summary>
        /// 處理 /help 事件
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task Help(Message message)
        {
            List<string> result = new List<string> { "指令選單:", ""};
            IEnumerable<(string, string)> menu = commandGenerator.GenerateMenuCommands();
            result.AddRange(menu.Select(command => $"{command.Item1, -15} {command.Item2}"));

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.Join('\n', result));
        }

        /// <summary>
        /// 處理 /bind 事件
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task Bind(Message message)
        {
            List<string> result = new List<string> { "綁定指令選單:", "" };
            IEnumerable<(string, string)> menu = await commandGenerator.GenerateBindCommandsAsync(message.Chat.Id);
            result.AddRange(menu.Select(command => $"{command.Item1,-15} {command.Item2}"));

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.Join('\n', result));
        }

        /// <summary>
        /// 處理預設指令以外的事件
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task Defult(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.Format("Hi @{0} 請使用 /help 獲得完整指令", message.Chat.Username));
        }
    }
}
