using MicrosoftGraphAPIBot.MicrosoftGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MicrosoftGraphAPIBot.Telegram
{
    public partial class TelegramHandler
    {
        /// <summary>
        /// 註冊新的應用程式到 Azure
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task RegisterApp(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: string.Format("註冊應用程式: [Get an app ID and secret]({0})", BindHandler.AppRegistrationUrl),
                ParseMode.MarkdownV2
            );

            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            string command = GetAsyncMethodCommand(MethodBase.GetCurrentMethod());

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: command + "\n" +
                    "[office365帳號] [Application (client) ID] [Client secrets]" + "\n" +
                    "AAA@BBB.onmicrosoft.com 9a448485-16dd-49c3-b4be-d8b7e138db27 lyfJ7f4k=9:qA?e:huHchb0pcBhMuk@b]",
                replyMarkup: new ForceReplyMarkup()
            );
        }

        /// <summary>
        /// 處理使用者回傳的 Azure App 訊息
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task ReplayRegisterApp(Message message)
        {
            try
            {
                string[] userMessages = message.Text.Split(' ');
                if (userMessages.Length != 3)
                    throw new InvalidOperationException("輸入格式錯誤");
                await bindHandler.RegAppAsync(message.Chat.Id, message.Chat.Username, userMessages[0], userMessages[1], userMessages[2]);

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "應用程式註冊成功"
                    );
            }
            catch(Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: ex.Message
                    );
            }
        }

        /// <summary>
        /// 刪除本地應用程式紀錄，並回傳 azure 應用程式網頁，讓使用者手動刪除應用程式
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task DeleteApp(Message message)
        {

        }

        /// <summary>
        /// 查詢已註冊的azure應用程式
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task QueryApp(Message message)
        {

        }

        /// <summary>
        /// 綁定 o365 使用者授權到指定的應用程式
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task BindUserAuth(Message message)
        {
            IReadOnlyList<Guid> appId = await bindHandler.GetAppsIdAsync(message.Chat.Id);

            IEnumerable<InlineKeyboardButton> keyboardButtons = appId.Select(Id => new InlineKeyboardButton { Text = Id.ToString(), CallbackData = Id.ToString() });
            var keyboardMarkup = new InlineKeyboardMarkup(keyboardButtons);

            string command = GetAsyncMethodCommand(MethodBase.GetCurrentMethod());

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: command + "\n" +
                    "選擇要授權的應用程式",
                replyMarkup: keyboardMarkup
            );
        }

        /// <summary>
        /// 刪除指定應用程式的 o365 使用者授權
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task UnbindUserAuth(Message message)
        {

        }

        /// <summary>
        /// 查詢使用者的所有 o365 授權
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task QueryUserAuth(Message message)
        {

        }

        /// <summary>
        /// 取得非同步 Method 對應的指令
        /// 
        /// 非同步 Method 名稱會包含 Thread Id
        /// </summary>
        /// <param name="asyncMethod"> Method </param>
        /// <returns></returns>
        private string GetAsyncMethodCommand(MethodBase asyncMethod)
        {
            string asyncMethodName = asyncMethod.DeclaringType.Name;
            int first = asyncMethodName.IndexOf("<") + "<".Length;
            int last = asyncMethodName.LastIndexOf(">");
            string methodName = asyncMethodName[first..last];

            string command = Controller.First(c => c.Value.Item1.Method.Name == methodName).Key;

            return command;
        }
    }
}
