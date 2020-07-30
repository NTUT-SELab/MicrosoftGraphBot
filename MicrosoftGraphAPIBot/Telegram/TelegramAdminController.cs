using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MicrosoftGraphAPIBot.Telegram
{
    public partial class TelegramController
    {
        private async Task RunAllApiTask(Message message)
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "執行中...請稍後");

            try
            {
                await apiCallManager.RunAsync();
                await botClient.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "執行完畢");
            }
            catch(Exception ex)
            {
                logger.LogError($"RunAllApiTask Failed: {ex.Message}");
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "執行失敗,請檢視Log");
            }
        }
    }
}
