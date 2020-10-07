using Hangfire.Storage.Monitoring;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace MicrosoftGraphAPIBot.Telegram
{
    public partial class TelegramController
    {
        /// <summary>
        /// 手動執行 Api 任務(所有使用者)
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task RunAllApiTask(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "執行中...請稍後");

            try
            {
                await hangfireJob.CallApiJob();
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "執行完畢");
            }
            catch(Exception ex)
            {
                logger.LogError($"[RunAllApiTask] Message: {ex.Message}");
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "執行失敗,請檢視Log");
            }
        }

        /// <summary>
        /// 推播呼叫 Api 的結果給使用者
        /// </summary>
        /// <param name="message"> Telegram message object </param>
        /// <returns></returns>
        private async Task RunPushApiResultTask(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "執行中...請稍後");

            try
            {
                await hangfireJob.PushApiResultJob();
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "執行完畢");
            }
            catch (Exception ex)
            {
                logger.LogError($"RunPushApiResultTask Failed: {ex.Message}");
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "執行失敗,請檢視Log");
            }
        }

        /// <summary>
        /// 管理員公告
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task SendAnn(Message message)
        {
            string command = GetAsyncMethodCommand(MethodBase.GetCurrentMethod());

            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: command + "\n" +
                    "請輸入要公告的內容",
                replyMarkup: new ForceReplyMarkup());
        }

        /// <summary>
        /// 管理員公告
        /// 
        /// 發送給所有使用者
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task SendAnnReplay(Message message)
        {
            try
            {
                IEnumerable<(long, string)> userIds = await telegramHandler.GetAllUserIdAsync();

                IEnumerable<Task> sendMessageTasks = userIds.Select(user => SendMessage(user.Item1, user.Item2 != string.Empty ? $"Hi @{user.Item2}, 管理員公告: {message.Text}" : $"管理員公告: {message.Text}"));
                await Task.WhenAll(sendMessageTasks);
            }
            catch (Exception ex)
            {
                Guid errorId = Guid.NewGuid();
                logger.LogError($"[SendAnnReplay] Error Id: {errorId}, Message: {ex.Message}");
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"發生錯誤, 錯誤 ID:{errorId}");
            }
        }
    }
}
