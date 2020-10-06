using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.Telegram
{
    /// <summary>
    ///  Telegram Bot 指令集
    /// </summary>
    public static class TelegramCommand
    {
        public const string Start = "/start";
        public const string Help = "/help";
        public const string Bind = "/bind";
        public const string Admin = "/admin";
        public const string RegApp = "/regApp";
        public const string DeleteApp = "/deleteApp";
        public const string QueryApp = "/queryApp";
        public const string BindAuth = "/bindAuth";
        public const string UnbindAuth = "/unbindAuth";
        public const string QueryAuth = "/queryAuth";
        public const string RebindAuth = "/rebindAuth";
        public const string RunApiTask = "/runApi";
        public const string RunAllApiTask = "/runAllApi";
        public const string PushApiResult = "/pushResults";
        public const string AddAdminPermission = "/addAdminPermission";
        public const string RemoveAdminPermission = "/removeAdminPermission";
    }

    /// <summary>
    /// 產生指令選單
    /// </summary>
    public class TelegramCommandGenerator
    {
        private readonly TelegramHandler telegramHandler;

        private static readonly Dictionary<string, string> instructions = new Dictionary<string, string>
        {
            { TelegramCommand.Start, "" },
            { TelegramCommand.Help, "指令選單" },
            { TelegramCommand.Bind, "綁定帳號" },
            { TelegramCommand.Admin, "管理者選單" },
            { TelegramCommand.RegApp, "註冊應用程式" },
            { TelegramCommand.DeleteApp, "刪除應用程式" },
            { TelegramCommand.QueryApp, "查詢應用程式" },
            { TelegramCommand.BindAuth, "綁定使用者授權到指定應用程式" },
            { TelegramCommand.UnbindAuth, "解除綁定使用者授權" },
            { TelegramCommand.RebindAuth, "重新綁定使用者授權到指定應用程式" },
            { TelegramCommand.QueryAuth, "查詢使用者授權" },
            { TelegramCommand.RunApiTask, "手動執行 Api 任務" },
            { TelegramCommand.RunAllApiTask, "手動執行 Api 任務(所有使用者)" },
            { TelegramCommand.PushApiResult, "推播呼叫 Api 的結果給使用者" },
            { TelegramCommand.AddAdminPermission, "新增管理員權限" },
            { TelegramCommand.RemoveAdminPermission, "移除管理員權限" },
        };

        public static readonly string[] AdminCommands = new string[] { TelegramCommand.RunAllApiTask, TelegramCommand.PushApiResult };

        /// <summary>
        /// Create a new TelegramCommandGenerator instance.
        /// </summary>
        /// <param name="telegramHandler"></param>
        public TelegramCommandGenerator(TelegramHandler telegramHandler) =>
            (this.telegramHandler) = (telegramHandler);

        /// <summary>
        /// 產生預設選單
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<(string, string)>> GenerateMenuCommandsAsync(long userId)
        {
            List<string> commands = new List<string> { TelegramCommand.Help, TelegramCommand.Bind };

            if (await telegramHandler.AuthCountAsync(userId) > 0)
                commands.Add(TelegramCommand.RunApiTask);

            if (await telegramHandler.CheckIsAdminAsync(userId))
                commands.AddRange(new string[] { TelegramCommand.Admin, TelegramCommand.RemoveAdminPermission });
            else
                commands.Add(TelegramCommand.AddAdminPermission);

            return commands.Select(command => (command, instructions[command]));
        }

        /// <summary>
        /// 產生綁定帳號選單
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<(string, string)>> GenerateBindCommandsAsync(long userId)
        {
            List<string> commands = new List<string> { TelegramCommand.RegApp };

            if (await telegramHandler.AppCountAsync(userId) > 0)
            {
                commands.AddRange(new List<string> { TelegramCommand.DeleteApp, TelegramCommand.QueryApp, TelegramCommand.BindAuth });

                if (await telegramHandler.AuthCountAsync(userId) > 0)
                    commands.AddRange(new List<string> { TelegramCommand.UnbindAuth, TelegramCommand.QueryAuth, TelegramCommand.RebindAuth });
            }

            return commands.Select(command => (command, instructions[command]));
        }

        /// <summary>
        /// 產生管理者選單
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(string, string)> GenerateAdminCommands()
        {
            return AdminCommands.Select(command => (command, instructions[command]));
        }
    }
}
