using Coaction.KickAssCardBot.Embed_Output;
using Coaction.KickAssCardBot.Manager;
using Coaction.KickAssCardBot.Services;
using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Coaction.KickAssCardBot
{
    public static class Startup
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddEnvironmentVariables();
                    config.AddUserSecrets(Assembly.GetEntryAssembly()!);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<DiscordSocketClient>(x => new DiscordSocketClient(new DiscordSocketConfig
                    {
                        GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent,
                        LogLevel = LogSeverity.Info,
                        MessageCacheSize = 100
                    }));
                    services.AddHostedService<BotService>();
                    services.AddSingleton<CommandHandlingService>();
                    services.AddSingleton<InteractionHandlingService>();
                    services.AddSingleton<CommandService>();
                    services.AddSingleton<InteractionService>();
                    services.AddSingleton<SpoilerStateStore>();
                    services.AddScoped<ScryfallManagerService>();
                    services.AddSingleton<MtgCardOutputManager>();
                    services.AddScoped<WizardsEventLocatorManager>();
                    services.AddScoped<GeoLocationApiManager>();
                    services.AddScoped<HtmlAgilityManager>();
                    services.AddScoped<MtgMeleeManager>();
                    services.AddHostedService<ScryfallSpoilerMonitoringService>();
                    services.AddMemoryCache();
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                });
    }
}
