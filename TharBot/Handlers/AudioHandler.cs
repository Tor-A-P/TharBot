using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace TharBot.Handlers
{
    public class AudioHandler : DiscordClientService
    {
        private readonly DiscordSocketClient _client;
        private readonly LavaNode _lavaNode;

        public AudioHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, LavaNode lavaNode) : base(client, logger)
        {
            _client = client;
            _lavaNode = lavaNode;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.Ready += OnClientReady;

            _lavaNode.OnTrackEnded += OnTrackEnded;
            _lavaNode.OnLog += LogAsync;
        }


        private async Task OnClientReady()
        {
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }
        }

        private async Task LogAsync(LogMessage logMessage)
        {
            await LoggingHandler.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message);
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (args.Reason == TrackEndReason.Replaced) return;
            if (args.Reason == TrackEndReason.Stopped) return;

            try
            {
                if (args.Player == null) return;
                if (args.Player.Queue == null) return;
                if (!args.Player.Queue.TryDequeue(out var queueable))
                {
                    await args.Player.TextChannel.SendMessageAsync("Playback Finished!");
                    return;
                }

                if (queueable is not LavaTrack track)
                {
                    await args.Player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                    return;
                }

                if (args.Reason is TrackEndReason.LoadFailed) await args.Player.TextChannel.SendMessageAsync("Loading song failed, trying next song!");

                await args.Player.PlayAsync(track);

                var embed = await EmbedHandler.CreateMusicEmbedBuilder("Now Playing:", $"{track.Title} / {track.Duration:%h\\:mm\\:ss}\n{track.Url}", args.Player, false);

                await args.Player.TextChannel.SendMessageAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Play", ex.Message);
                await args.Player.TextChannel.SendMessageAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("Victoria", null, ex);
            }
        }
    }
}
