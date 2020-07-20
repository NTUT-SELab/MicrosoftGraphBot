using MicrosoftGraphAPIBot.MicrosoftGraph;
using MicrosoftGraphAPIBot.Models;
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
                text: $"註冊應用程式: [Get an app ID and secret]({BindHandler.AppRegistrationUrl})",
                ParseMode.MarkdownV2);

            await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);
            string command = GetAsyncMethodCommand(MethodBase.GetCurrentMethod());

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: command + "\n" +
                    "[office365帳號] [Application (client) ID] [Client secrets] [應用程式別名 (用於管理)]" + "\n" +
                    "範例: AAA@BBB.onmicrosoft.com 9a448485-16dd-49c3-b4be-d8b7e138db27 lyfJ7f4k=9:qA?e:huHchb0pcBhMuk@b App1]" + "\n" +
                    "備註: 每個項目請用空格分開",
                replyMarkup: new ForceReplyMarkup());
        }

        /// <summary>
        /// 處理使用者回傳的 Azure App 訊息
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task RegisterAppReplay(Message message)
        {
            try
            {
                string[] userMessages = message.Text.Split(' ');
                if (userMessages.Length != 4)
                    throw new InvalidOperationException("輸入格式錯誤");
                await bindHandler.RegAppAsync(message.Chat.Id, message.Chat.Username, userMessages[0], userMessages[1], userMessages[2], userMessages[3]);

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "應用程式註冊成功");

                await BindUserAuth(message);
            }
            catch(Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: ex.Message);

                await Bind(message);
            }
        }

        /// <summary>
        /// 刪除本地應用程式紀錄，並回傳 azure 應用程式網頁，讓使用者手動刪除應用程式
        /// 
        /// 提供使用者選擇應用程式
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task DeleteApp(Message message)
        {
            var keyboardMarkup = await GetUserAppsNameAsync(message.Chat.Id);
            string command = GetAsyncMethodCommand(MethodBase.GetCurrentMethod());

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: command + "\n" +
                    "選擇要刪除的應用程式",
                replyMarkup: keyboardMarkup);
        }

        /// <summary>
        /// 刪除本地應用程式紀錄，並回傳 azure 應用程式網頁，讓使用者手動刪除應用程式
        /// 
        /// 執行刪除動作
        /// </summary>
        /// <param name="callbackQuery"> Telegram callbackQuery object </param>
        /// <returns></returns>
        private async Task DeleteAppCallback(CallbackQuery callbackQuery)
        {
            string deleteUrl = await bindHandler.DeleteAppAsync(callbackQuery.Data);

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.From.Id,
                text: $"本地應用程式關聯已刪除，請點擊後方連結至 Azure 刪除應用程式: [Azure 應用程式連結]({deleteUrl})",
                ParseMode.MarkdownV2);
        }

        /// <summary>
        /// 查詢已註冊的 azure 應用程式
        /// 
        /// 提供使用者選擇應用程式
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task QueryApp(Message message)
        {
            var keyboardMarkup = await GetUserAppsNameAsync(message.Chat.Id);
            string command = GetAsyncMethodCommand(MethodBase.GetCurrentMethod());

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: command + "\n" +
                    "選擇要查詢的應用程式",
                replyMarkup: keyboardMarkup);
        }

        /// <summary>
        /// 查詢已註冊的 azure 應用程式
        /// 
        /// 列出應用程式詳細訊息
        /// </summary>
        /// <param name="callbackQuery"> Telegram callbackQuery object </param>
        /// <returns></returns>
        private async Task QueryAppCallback(CallbackQuery callbackQuery)
        {
            AzureApp app = await bindHandler.GetAppInfoAsync(callbackQuery.Data);
            string[] infos = new string[] { 
                $"應用程式 (用戶端) 識別碼: {app.Id}",
                $"應用程式別名: {app.Name}",
                $"Client secrets: {app.Secrets}",
                $"註冊應用程式使用的信箱: {app.Email}",
                $"註冊應用程式時間: {app.RegTime}"
            };
            string text = string.Join('\n', infos);

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.From.Id,
                text: text);
        }

        /// <summary>
        /// 對指定應用程式取得 o365 帳號授權
        /// 
        /// 提供使用者選擇應用程式
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task BindUserAuth(Message message)
        {
            var keyboardMarkup = await GetUserAppsNameAsync(message.Chat.Id);
            string command = GetAsyncMethodCommand(MethodBase.GetCurrentMethod());

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: command + "\n" +
                    "選擇要授權的應用程式",
                replyMarkup: keyboardMarkup);
        }

        /// <summary>
        /// 對指定應用程式取得 o365 帳號授權
        /// 
        /// 產生授權連結
        /// </summary>
        /// <param name="callbackQuery"> Telegram callbackQuery object </param>
        /// <returns></returns>
        private async Task BindUserAuthCallback(CallbackQuery callbackQuery)
        {
            (string, string) auth = await bindHandler.GetAuthUrlAsync(callbackQuery.Data);

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.From.Id,
                text: $"授權帳號: [授權連結]({auth.Item2})",
                ParseMode.MarkdownV2);

            string command = GetAsyncMethodCommand(MethodBase.GetCurrentMethod());

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.From.Id,
                text: command + "\n" +
                    $"應用程式Id: {auth.Item1}" + "\n" +
                    "[網頁內容] [授權別名 (用於管理)]" + "\n" +
                    @"範例: {""code"":""asf754...""} Auth1" + "\n" +
                    "備註: 每個項目請用空格分開",
                replyMarkup: new ForceReplyMarkup());
        }

        /// <summary>
        /// 對指定應用程式取得 o365 帳號授權
        /// 
        /// 綁定授權程序
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task BindUserAuthReplay(Message message)
        {
            try
            {
                string[] userMessages = message.Text.Split(' ');
                if (userMessages.Length != 2)
                    throw new InvalidOperationException("輸入格式錯誤");

                string clientIdItem = message.ReplyToMessage.Text.Split('\n')[1];
                string clientId = clientIdItem.Split(' ')[1];
                await bindHandler.BindAuthAsync(clientId, userMessages[0], userMessages[1]);

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "授權綁定成功");
            }
            catch(Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: ex.Message);
            }
        }

        /// <summary>
        /// 刪除指定應用程式的 o365 使用者授權
        /// 
        /// 提供使用者選擇 o365 使用者授權
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task UnbindUserAuth(Message message)
        {
            var keyboardMarkup = await GetUserAuthsNameAsync(message.Chat.Id);
            string command = GetAsyncMethodCommand(MethodBase.GetCurrentMethod());

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: command + "\n" +
                    "選擇要刪除的授權",
                replyMarkup: keyboardMarkup);
        }

        /// <summary>
        /// 刪除指定應用程式的 o365 使用者授權
        /// 
        /// 執行刪除動作
        /// </summary>
        /// <param name="callbackQuery"> Telegram callbackQuery object </param>
        /// <returns></returns>
        private async Task UnbindUserAuthCallback(CallbackQuery callbackQuery)
        {
            await bindHandler.UnbindAuthAsync(callbackQuery.Data);

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.From.Id,
                text: $"已成功刪除應用程式授權");
        }

        /// <summary>
        /// 查詢使用者的所有 o365 授權
        /// 
        /// 提供使用者選擇 o365 授權
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task QueryUserAuth(Message message)
        {
            var keyboardMarkup = await GetUserAuthsNameAsync(message.Chat.Id);
            string command = GetAsyncMethodCommand(MethodBase.GetCurrentMethod());

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: command + "\n" +
                    "選擇要查詢的授權",
                replyMarkup: keyboardMarkup);
        }

        /// <summary>
        /// 查詢使用者的所有 o365 授權
        /// 
        /// 列出 o365 授權的詳細訊息
        /// </summary>
        /// <param name="callbackQuery"> Telegram callbackQuery object </param>
        /// <returns></returns>
        private async Task QueryUserAuthCallback(CallbackQuery callbackQuery)
        {
            AppAuth auth = await bindHandler.GetAuthInfoAsync(callbackQuery.Data);
            string[] infos = new string[] {
                $"授權識別碼: {auth.Id}",
                $"授權別名: {auth.Name}",
                $"Refresh token: {auth.RefreshToken}",
                $"Scope: {auth.Scope}",
                $"綁定時間: {auth.BindTime}",
                $"Token 更新的時間: {auth.UpdateTime}"
            };
            string text = string.Join('\n', infos);

            await botClient.SendTextMessageAsync(
                chatId: callbackQuery.From.Id,
                text: text);
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

            string command = Controller.First(c => (c.Value.Item1 != null && c.Value.Item1.Method.Name == methodName) ||
                                                    (c.Value.Item2 != null && c.Value.Item2.Method.Name == methodName) ||
                                                    (c.Value.Item3 != null && c.Value.Item3.Method.Name == methodName)).Key;

            return command;
        }

        /// <summary>
        /// 取得 Telegram 使用者已註冊的應用程式名稱
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <returns></returns>
        private async Task<InlineKeyboardMarkup> GetUserAppsNameAsync(long userId)
        {
            IEnumerable<(Guid, string)> appsInfo = await bindHandler.GetAppsNameAsync(userId);

            IEnumerable<InlineKeyboardButton> keyboardButtons = appsInfo.Select(appInfo => InlineKeyboardButton.WithCallbackData(appInfo.Item2.ToString(), appInfo.Item1.ToString()));
            return new InlineKeyboardMarkup(keyboardButtons);
        }

        /// <summary>
        /// 取得 Telegram 使用者綁定的授權名稱
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <returns></returns>
        private async Task<InlineKeyboardMarkup> GetUserAuthsNameAsync(long userId)
        {
            IEnumerable<(Guid, string)> authsInfo = await bindHandler.GetAuthsNameAsync(userId);

            IEnumerable<InlineKeyboardButton> keyboardButtons = authsInfo.Select(authInfo => InlineKeyboardButton.WithCallbackData(authInfo.Item2.ToString(), authInfo.Item1.ToString()));
            return new InlineKeyboardMarkup(keyboardButtons);
        }
    }
}
