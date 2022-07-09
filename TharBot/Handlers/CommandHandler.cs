using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

        private async Task OnCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext commandContext, IResult result)
        {
            if (result.IsSuccess)
            {
                await LoggingHandler.LogInformationAsync("bot", $"Executed command \"{commandInfo.Value.Name}\"!");
                return;
            }

            var existingPrefix = db.LoadRecordById<Prefixes>("Prefixes", commandContext.Guild.Id);
            string? prefix;
            if (existingPrefix != null) prefix = existingPrefix.Prefix;
            else prefix = _configuration["Prefix"];

            if (result.Error == CommandError.UnknownCommand)
            {
                var memeList = db.LoadRecordById<MemeCommands>("Memes", commandContext.Guild.Id);
                if (memeList != null)
                {
                    if (memeList.Memes.ContainsKey(commandContext.Message.ToString().Replace($"{prefix}", "")))
                    {
                        await commandContext.Channel.SendMessageAsync(memeList.Memes[commandContext.Message.ToString()
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
            if (message.Source != MessageSource.User) return;
            var existingBan = db.LoadRecordById<BannedUser>("UserBanlist", socketMessage.Author.Id);
            if (existingBan != null) return;
            var forGuildId = socketMessage.Channel as SocketGuildChannel;

            var existingPrefix = db.LoadRecordById<Prefixes>("Prefixes", forGuildId.Guild.Id);
            string? prefix;
            if (existingPrefix != null) prefix = existingPrefix.Prefix;
            else prefix = _configuration["Prefix"];

            var existingWLRec = db.LoadRecordById<Whitelist>("WhitelistedChannels", forGuildId.Guild.Id);
            if (existingWLRec != null)
            {
                if (existingWLRec.WLChannelId.Any())
                {
                    if (!existingWLRec.WLChannelId.Contains(socketMessage.Channel.Id) && (message.Content != $"{prefix}wlc")) return;
                }
            }

            var existingBLRec = db.LoadRecordById<Blacklist>("BlacklistedChannels", forGuildId.Guild.Id);
            if (existingBLRec != null)
            {
                if (existingBLRec.BLChannelId.Any())
                {
                    if (existingBLRec.BLChannelId.Contains(socketMessage.Channel.Id) && (message.Content != $"{prefix}blc")) return;
                }
            }

            var argPos = 0;
            if (!message.HasStringPrefix(prefix, ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;

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

