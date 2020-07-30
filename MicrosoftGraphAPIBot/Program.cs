﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.MicrosoftGraph;
using MicrosoftGraphAPIBot.Models;
using MicrosoftGraphAPIBot.Services;
using MicrosoftGraphAPIBot.Telegram;
using Telegram.Bot;

namespace MicrosoftGraphAPIBot
{
    static class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
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
                    string DBConnection = Utils.GetDBConnection(hostContext.Configuration);
                    services.AddDbContext<BotDbContext>(options =>
                    {
                        options.UseSqlServer(DBConnection);
                    });
                    services.AddHttpClient();
                    services.AddScoped<ITelegramBotClient>(service => new TelegramBotClient(hostContext.Configuration["Telegram:Token"]));
                    services.AddScoped<BindHandler>();
                    services.AddScoped<GraphApi, OutlookApi>();
                    services.AddTransient<ApiController>();
                    services.AddScoped<ApiCallManager>();
                    services.AddScoped<DefaultGraphApi>();
                    services.AddScoped<TelegramController>();
                    services.AddScoped<TelegramHandler>();
                    services.AddScoped<TelegramCommandGenerator>();
                    services.AddHostedService<StartupService>();
                    services.AddHostedService<TelegramBotService>();
                    services.AddHostedService<ApiCallService>();
                });
    }
}
