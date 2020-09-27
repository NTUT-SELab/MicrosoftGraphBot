using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using MicrosoftGraphAPIBot.Models;
using MicrosoftGraphAPIBot.Services;
using MicrosoftGraphAPIBot.Telegram;
using System;
using Telegram.Bot;

namespace MicrosoftGraphAPIBot
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
#if DEBUG
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddFile("Logs/{Date}.txt", LogLevel.Debug);
#else
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddFile("Logs/{Date}.txt", LogLevel.Information);
#endif
                })
                .ConfigureServices((hostContext, services) =>
                {
                    if (!Utils.CheckConfig(hostContext.Configuration))
                        throw new BotException("Configuration file check failed.");
                    string DBConnection = Utils.GetDBConnection(hostContext.Configuration);
                    services.AddDbContext<BotDbContext>(options =>
                    {
                        options.UseSqlServer(DBConnection);
                    });
                    services.AddHttpClient();
                    services.AddScoped<ITelegramBotClient>(service => new TelegramBotClient(hostContext.Configuration["Telegram:Token"]));
                    services.AddScoped<BindHandler>();
                    services.AddScoped<GraphApi, OutlookApi>();
                    services.AddScoped<GraphApi, OneDriveApi>();
                    services.AddScoped<GraphApi, PermissionsApi>();
                    services.AddScoped<GraphApi, CalendarApi>();
                    services.AddScoped<GraphApi, PersonalContactsApi>();
                    services.AddTransient<ApiController>();
                    services.AddTransient<ApiCallManager>();
                    services.AddScoped<DefaultGraphApi>();
                    services.AddScoped<HangfireJob>();
                    services.AddScoped<TelegramController>();
                    services.AddScoped<TelegramHandler>();
                    services.AddScoped<TelegramCommandGenerator>();
                    services.AddHostedService<StartupService>();
                    services.AddHostedService<TelegramBotService>();
                    services.AddHostedService<CrontabService>();
                });
    }
}
