using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.Telegram
{
    public class TelegramHandler
    {
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly BotDbContext db;

        public TelegramHandler(ILogger<TelegramHandler> logger, IConfiguration configuration, BotDbContext db) =>
            (this.logger, this.configuration, this.db) = (logger, configuration, db);

        /// <summary>
        /// 檢查使用者是否有管理者權限
        /// </summary>
        /// <param name="telegramId"> Telegram user id </param>
        /// <returns></returns>
        public async Task<bool> CheckIsAdmin(long telegramId)
        {
            if (await db.TelegramUsers.AsQueryable().Where(user => user.Id == telegramId && user.IsAdmin).CountAsync() == 1)
                return true;
            return false;
        }

        /// <summary>
        /// 新增管理者權限
        /// </summary>
        /// <param name="telegramId"> Telegram user id </param>
        /// <param name="userName"> Telegram user name </param>
        /// <param name="password"> 用於驗證管理者身份的密碼 </param>
        /// <returns></returns>
        public async Task<bool> AddAdminPermission(long telegramId, string userName, string password)
        {
            if (password == configuration["AdminPassword"])
            {
                TelegramUser telegramUser = await db.TelegramUsers.FindAsync(telegramId);
                if (telegramUser == null)
                {
                    telegramUser = new TelegramUser { Id = telegramId, UserName = userName, IsAdmin = true };
                    db.TelegramUsers.Add(telegramUser);
                }
                else
                {
                    telegramUser.UserName = userName;
                    telegramUser.IsAdmin = true;
                    db.TelegramUsers.Update(telegramUser);
                }

                logger.LogInformation($"Telegram user: {telegramId}(@{userName}) 升級管理者權限:成功");
                await db.SaveChangesAsync();
                return true;
            }

            logger.LogWarning($"Telegram user: {telegramId}(@{userName}) 升級管理者權限:失敗-驗證失敗");
            return false;
        }

        /// <summary>
        /// 移除管理者權限
        /// </summary>
        /// <param name="telegramId"> Telegram user id </param>
        /// <param name="userName"> Telegram user name </param>
        /// <returns></returns>
        public async Task RemoveAdminPermission(long telegramId, string userName)
        {
            TelegramUser telegramUser = await db.TelegramUsers.FindAsync(telegramId);
            if (telegramUser == null)
            {
                telegramUser = new TelegramUser { Id = telegramId, UserName = userName };
                db.TelegramUsers.Add(telegramUser);
            }
            else
            {
                telegramUser.UserName = userName;
                telegramUser.IsAdmin = false;
                db.TelegramUsers.Update(telegramUser);
            }

            await db.SaveChangesAsync();
        }
    }
}
