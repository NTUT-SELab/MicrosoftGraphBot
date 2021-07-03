using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicrosoftGraphAPIBot.Services
{
    public class CrontabService : IHostedService
    {
        private readonly IHost host;
        private readonly ILogger logger;
        private readonly IConfiguration configuration;
        private readonly IServiceProvider serviceProvider;
        private readonly HangfireJob hangfireJob;
        private BackgroundJobServer backgroundJobServer;
        private bool isStart = false;

        public CrontabService(IHost host, ILogger<CrontabService> logger, IConfiguration configuration, IServiceProvider serviceProvider, HangfireJob hangfireJob) =>
            (this.host, this.logger, this.configuration, this.serviceProvider, this.hangfireJob) = (host, logger, configuration, serviceProvider, hangfireJob);

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Utils.DeleteDelayTime = configuration.GetValue<int>("DeleteDelayTime");
                StartReceiving();
                isStart = true;
                logger.LogInformation("Api call service is starting.");
            }
            catch(Exception ex)
            {
                logger.LogError(ex.Message);
                await host.StopAsync().ConfigureAwait(false);
            }
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
                .UseActivator(new HangfireActivator(serviceProvider))
                .UseSqlServerStorage(DBConnection, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                });

            using (var connection = JobStorage.Current.GetConnection())
                foreach (var recurringJob in connection.GetRecurringJobs())
                    RecurringJob.RemoveIfExists(recurringJob.Id);
            RecurringJob.AddOrUpdate(() => hangfireJob.CallApiJob(), configuration["Cron"], TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(() => hangfireJob.CheckVerJob(), configuration["CheckVerCron"], TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate(() => hangfireJob.PushApiResultJob(), configuration["PushResultCron"], TimeZoneInfo.Local);

            backgroundJobServer = new BackgroundJobServer();
        }

        private void StopReceiving()
        {
            backgroundJobServer.Dispose();
        }
    }

    public class HangfireActivator : JobActivator
    {
        private readonly IServiceProvider serviceProvider;

        public HangfireActivator(IServiceProvider serviceProvider) =>
            this.serviceProvider = serviceProvider;

        public override object ActivateJob(Type jobType)
        {
            return serviceProvider.GetService(jobType);
        }
    }

    public class HangfireJob
    {
        private readonly IServiceProvider serviceProvider;

        public HangfireJob(IServiceProvider serviceProvider) =>
            this.serviceProvider = serviceProvider;

        public async Task CallApiJob()
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            IServiceProvider scopeServiceProvider = scope.ServiceProvider;
            ApiCallManager apiCallManager = scopeServiceProvider.GetService(typeof(ApiCallManager)) as ApiCallManager;
            await apiCallManager.RunAsync();
        }

        public async Task CheckVerJob()
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            IServiceProvider scopeServiceProvider = scope.ServiceProvider;
            await Utils.CheckAppVersionAsync(scopeServiceProvider);
        }

        public async Task PushApiResultJob()
        {
            using IServiceScope scope = this.serviceProvider.CreateScope();
            IServiceProvider scopeServiceProvider = scope.ServiceProvider;
            await Utils.PushApiResultAsync(scopeServiceProvider);
        }
    }
}
