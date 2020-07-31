using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.Models;
using System;
using System.Collections.Generic;
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
        public async Task<bool> CheckIsAdminAsync(long telegramId)
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
        public async Task<bool> AddAdminPermissionAsync(long telegramId, string userName, string password)
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
        public async Task RemoveAdminPermissionAsync(long telegramId, string userName)
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

        #region Azure app

        /// <summary>
        /// 取得指定 Telegram 使用者註冊的應用程式數量
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <returns> 應用程式數量 </returns>
        public async Task<int> AppCountAsync(long userId)
        {
            return await db.AzureApps.AsQueryable()
                .Where(app => app.TelegramUserId == userId)
                .CountAsync();
        }

        /// <summary>
        /// 取得指定 Telegram 使用者註冊的應用程式別名
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <returns> 應用程式別名 </returns>
        public async Task<IEnumerable<(Guid, string)>> GetAppsNameAsync(long userId)
        {
            var appInfos = await db.AzureApps.AsQueryable()
                .Where(app => app.TelegramUser.Id == userId)
                .Select(app => new { app.Id, app.Name })
                .ToListAsync();
            return appInfos.Select(app => (app.Id, app.Name));
        }

        /// <summary>
        /// 取得指定的應用程式資訊
        /// </summary>
        /// <param name="clientId"> Application (client) ID </param>
        /// <returns> 應用程式資訊 </returns>
        public async Task<AzureApp> GetAppInfoAsync(string clientId)
        {
            Guid id = Guid.Parse(clientId);
            var appInfo = await db.AzureApps.FindAsync(id);

            return appInfo;
        }

        #endregion

        #region App auth

        /// <summary>
        /// 取得指定 Telegram 使用者綁定的授權數量
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <returns> 授權數量 </returns>
        public async Task<int> AuthCountAsync(long userId)
        {
            return await db.AppAuths.AsQueryable()
                .Where(auth => auth.AzureApp.TelegramUserId == userId)
                .CountAsync();
        }

        /// <summary>
        /// 取得指定 Telegram 使用者註冊的授權別名
        /// </summary>
        /// <param name="userId"> Telegram user id </param>
        /// <returns> 授權別名 </returns>
        public async Task<IEnumerable<(Guid, string)>> GetAuthsNameAsync(long userId)
        {
            var appInfos = await db.AppAuths.AsQueryable()
                .Where(auth => auth.AzureApp.TelegramUserId == userId)
                .Select(auth => new { auth.Id, auth.Name })
                .ToListAsync();
            return appInfos.Select(auth => (auth.Id, auth.Name));
        }

        /// <summary>
        /// 取得指定的授權資訊
        /// </summary>
        /// <param name="AppId"> 授權 id </param>
        /// <returns> 授權資訊 </returns>
        public async Task<AppAuth> GetAuthInfoAsync(string authId)
        {
            Guid id = Guid.Parse(authId);
            var appInfo = await db.AppAuths.FindAsync(id);

            return appInfo;
        }

        #endregion
    }
}
