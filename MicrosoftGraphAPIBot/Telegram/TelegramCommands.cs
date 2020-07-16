using MicrosoftGraphAPIBot.MicrosoftGraph;
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
        public const string RegApp = "/regApp";
        public const string DeleteApp = "/deleteApp";
        public const string QueryApp = "/queryApp";
        public const string BindAuth = "/bindAuth";
        public const string UnbindAuth = "/unbindAuth";
        public const string QueryAuth = "/queryAuth";
    }

    /// <summary>
    /// 產生指令選單
    /// </summary>
    public class TelegramCommandGenerator
    {
        private readonly BindHandler bindHandler;

        private static readonly Dictionary<string, string> instructions = new Dictionary<string, string>
        {
            { TelegramCommand.Start, "" },
            { TelegramCommand.Help, "指令選單" },
            { TelegramCommand.Bind, "綁定帳號" },
            { TelegramCommand.RegApp, "註冊應用程式" },
            { TelegramCommand.DeleteApp, "刪除應用程式" },
            { TelegramCommand.QueryApp, "查詢應用程式" },
            { TelegramCommand.BindAuth, "綁定使用者授權到指定應用程式" },
            { TelegramCommand.UnbindAuth, "解除綁定使用者授權" },
            { TelegramCommand.QueryAuth, "查詢使用者授權" }
        };

        /// <summary>
        /// Create a new TelegramCommandGenerator instance.
        /// </summary>
        /// <param name="bindHandler"></param>
        public TelegramCommandGenerator(BindHandler bindHandler)
        {
            this.bindHandler = bindHandler;
        }

        /// <summary>
        /// 產生預設選單
        /// </summary>
        /// <returns></returns>
        public IEnumerable<(string, string)> GenerateMenuCommands()
        {
            List<string> commands = new List<string> { TelegramCommand.Help, TelegramCommand.Bind };
             
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

            if (await bindHandler.AppCountAsync(userId) > 0)
            {
                commands.AddRange(new List<string> { TelegramCommand.DeleteApp, TelegramCommand.QueryApp, TelegramCommand.BindAuth });

                if (await bindHandler.AuthCountAsync(userId) > 0)
                    commands.AddRange(new List<string> { TelegramCommand.UnbindAuth, TelegramCommand.QueryAuth });
            }

            return commands.Select(command => (command, instructions[command]));
        }
    }
}
