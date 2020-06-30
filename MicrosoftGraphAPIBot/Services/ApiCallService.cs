using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.Services
{
    public class ApiCallService : IHostedService
    {
        private readonly IHost host;
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly ApiCallManager apiCallManager;
        private BackgroundJobServer backgroundJobServer;
        private bool isStart = false;

        public ApiCallService(IHost host, ILogger<ApiCallService> logger, IConfiguration configuration, ApiCallManager apiCallManager) =>
            (this.host, this.logger, this.configuration, this.apiCallManager) = (host, logger, configuration, apiCallManager);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                StartReceiving();
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
                await host.StopAsync().ConfigureAwait(false);
            }
            
            isStart = true;
            logger.LogInformation("Api call service is starting.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (isStart)
                StopReceiving();
            logger.LogInformation("Api call service is stopping.");

            return Task.CompletedTask;
        }

        private void StartReceiving()
        {
            string DBConnection = Utils.GetDBConnection(configuration);
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseColouredConsoleLogProvider()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(DBConnection, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                });

            RecurringJob.AddOrUpdate(() => apiCallManager.Run(), configuration["Cron"]);

            backgroundJobServer = new BackgroundJobServer();
        }

        private void StopReceiving()
        {
            backgroundJobServer.Dispose();
        }
    }
}
