using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MicrosoftGraphAPIBot.Telegram;

namespace MicrosoftGraphAPIBot
{
    class Program
    {
        static void Main(string[] args)
        {

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.AddFile("Logs/{Date}.txt");
#if DEBUG
                    logging.SetMinimumLevel(LogLevel.Debug);
#else
                    logging.SetMinimumLevel(LogLevel.Information);
#endif
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<TelegramHandler>();
                    services.AddHostedService<TelegramBotService>();
                });
    }
}
