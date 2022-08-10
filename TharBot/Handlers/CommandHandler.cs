using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Reflection;
using TharBot.DBModels;

namespace TharBot.Handlers
{
    public class CommandHandler : DiscordClientService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration configuration, ILogger<DiscordClientService> logger)
            : base(client, logger)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", _configuration);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.MessageReceived += OnMessageReceived;
            _service.CommandExecuted += OnCommandExecuted;

            _client.Ready += OnClientReady;
            _client.Log += LogAsync;

            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnCommandExecuted(Discord.Optional<CommandInfo> commandInfo, ICommandContext commandContext, IResult result)
        {
            var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", commandContext.Guild.Id);

            
            if (result.IsSuccess)
            {
                await LoggingHandler.LogInformationAsync("bot", $"Executed command \"{commandInfo.Value.Name}\"!");
                if (commandInfo.Value != null)
                {
                    if (commandInfo.Value.Remarks != null)
                    {
                        if (commandInfo.Value.Remarks.ToLower() == "music")
                        {
                            serverSettings.LastChannelUsedId = commandContext.Channel.Id;
                            var update = Builders<ServerSpecifics>.Update.Set(x => x.LastChannelUsedId, serverSettings.LastChannelUsedId);
                            await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", commandContext.Guild.Id, update);
                        }
                    }
                }
                return;
            }

            string? prefix;
            if (serverSettings.Prefix != null) prefix = serverSettings.Prefix;
            else prefix = _configuration["Prefix"];
            
            if (result.Error == CommandError.UnknownCommand)
            {
                if (commandContext.User.Id == Client.CurrentUser.Id) return;
                var memeList = serverSettings.Memes;
                if (memeList != null)
                {
                    if (memeList.ContainsKey(commandContext.Message.ToString().Replace($"{prefix}", "")))
                    {
                        await commandContext.Channel.SendMessageAsync(memeList[commandContext.Message.ToString()
                            .Replace($"{prefix}", "")]);
                        await LoggingHandler.LogInformationAsync("bot", $"Executed custom meme " +
                            $"\"{commandContext.Message.ToString().Replace($"{prefix}", "")}\"!");
                    }
                    else await commandContext.Channel.SendMessageAsync($"No command called {commandContext.Message.ToString().Replace($"{prefix}", "")}!");
                }
                else await commandContext.Channel.SendMessageAsync($"No command called {commandContext.Message.ToString().Replace($"{prefix}", "")}!");
            }
            else
            {
                var embed = await EmbedHandler.CreateErrorEmbed(commandContext.Message.ToString().Replace($"{prefix}", ""), result.ErrorReason);
                await commandContext.Channel.SendMessageAsync(embed: embed);
                await LoggingHandler.LogAsync($"COMND: {commandInfo.Value.Name}", LogSeverity.Warning, result.ErrorReason);
            }
        }

        private async Task OnMessageReceived(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message) return;
            var existingBan = await db.LoadRecordByIdAsync<BannedUser>("UserBanlist", socketMessage.Author.Id);
            if (existingBan != null) return;
            var forGuildId = socketMessage.Channel as SocketGuildChannel;

            var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", forGuildId.Guild.Id);
            string? prefix;
            if (serverSettings == null) return;
            if (serverSettings.Prefix != null) prefix = serverSettings.Prefix;
            else prefix = _configuration["Prefix"];

            if (serverSettings.WLChannelId != null)
            {
                if (serverSettings.WLChannelId.Any())
                {
                    if (!serverSettings.WLChannelId.Contains(socketMessage.Channel.Id) && (message.Content != $"{prefix}wlc")) return;
                }
            }

            if (serverSettings.BLChannelId != null)
            {
                if (serverSettings.BLChannelId.Any())
                {
                    if (serverSettings.BLChannelId.Contains(socketMessage.Channel.Id) && (message.Content != $"{prefix}blc")) return;
                }
            }

            var argPos = 0;
            if (!message.HasStringPrefix(prefix, ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnClientReady()
        {
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            await LoggingHandler.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message);
        }
    }
}

