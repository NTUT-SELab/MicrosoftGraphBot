using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot
{
    /// <summary>
    /// 程式初始化用的服務
    /// </summary>
    public class Startup : IHostedService
    {
        private readonly IHost host;
        private readonly ILogger logger;
        private readonly BotDbContext db;

        /// <summary>
        /// Create a new Startup instance.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="logger"></param>
        /// <param name="botDbContext"></param>
        public Startup(IHost host, ILogger<Startup> logger, BotDbContext botDbContext)
        {
            this.host = host;
            this.logger = logger;
            this.db = botDbContext;
        }

        /// <summary>
        /// 啟動服務
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Startup service is starting.");

            try
            {
                db.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                await host.StopAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 停止服務
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Startup service is stopping.");

            return Task.CompletedTask;
        }
    }
}
