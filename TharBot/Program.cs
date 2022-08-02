using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TharBot.Handlers;
using Victoria;

var builder = new HostBuilder()
        .ConfigureAppConfiguration(x =>
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.development.json", false, true)
            .Build();

            x.AddConfiguration(configuration);
        })
        .ConfigureLogging(x =>
        {
            x.AddConsole();
            x.SetMinimumLevel(LogLevel.Debug);
        })
        .ConfigureDiscordHost((context, config) =>
        {
            config.SocketConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Critical,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 200,
                GatewayIntents = GatewayIntents.All,
            };
            config.Token = context.Configuration["Token"];
        })
        .UseCommandService((context, config) =>
        {
            config.LogLevel = LogSeverity.Info;
            config.CaseSensitiveCommands = false;
            config.DefaultRunMode = RunMode.Async;
        })
        .ConfigureServices((context, services) =>
        {
            services
            .AddHostedService<CommandHandler>()
            .AddHostedService<AudioHandler>()
            .AddHostedService<ReactionsHandler>()
            .AddHostedService<ScheduledEventsHandler>()
            .AddHostedService<MessageHandler>()
            .AddHostedService<FightHandler>()
            .AddHttpClient()
            .AddLavaNode(x =>
             {
                 x.SelfDeaf = false;
                 x.Hostname = "127.0.0.1";
             });
        })
        .UseConsoleLifetime();



var host = builder.Build();

using (host)
{
    await host.RunAsync();
}
