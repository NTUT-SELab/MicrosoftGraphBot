using MicrosoftGraphAPIBot.MicrosoftGraph;
using System;
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

            string methodName = GetAsyncMethodName(MethodBase.GetCurrentMethod());
            string command = commands.First(c => c.Value.Item2.Method.Name == methodName).Key;

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
                    throw new Exception("輸入格式錯誤");
                await bindHandler.RegAppAsync(message, userMessages[0], userMessages[1], userMessages[2]);

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
        /// 取得正確的 Method 名稱
        /// 
        /// 非同步 Method 名稱會包含 Thread Id
        /// </summary>
        /// <param name="asyncMethod"> Method </param>
        /// <returns></returns>
        private string GetAsyncMethodName(MethodBase asyncMethod)
        {
            string asyncMethodName = asyncMethod.DeclaringType.Name;
            int first = asyncMethodName.IndexOf("<") + "<".Length;
            int last = asyncMethodName.LastIndexOf(">");
            string methodName = asyncMethodName[first..last];

            return methodName;
        }
    }
}
