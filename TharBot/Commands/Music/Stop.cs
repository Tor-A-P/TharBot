using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;
using Victoria;
using Victoria.Node;
using Victoria.Player;

namespace TharBot.Commands
{
    public class Stop : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Stop(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        [Command("Stop")]
        [Summary("Causes the bot to stop the player, clear the queue, and leave the voice channel\n" +
            "**USAGE:** th.stop")]
        [Remarks("Music")]
        public async Task StopAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                var notPlayingEmbed = await EmbedHandler.CreateUserErrorEmbed("Stop", "I'm not connected to a voice channel!");
                await ReplyAsync(embed: notPlayingEmbed);
                return;
            }

            var commandUser = Context.User as SocketGuildUser;
            var bot = Context.Guild.GetUser(Context.Client.CurrentUser.Id);

            if (commandUser.VoiceChannel != bot.VoiceChannel)
            {
                var wrongVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Stop", "You must be connected to the same voice channel as the bot!");
                await ReplyAsync(embed: wrongVCEmbed);
                return;
            }

            if (player.PlayerState == PlayerState.Stopped)
            {
                var alreadyStoppedEmbed = await EmbedHandler.CreateUserErrorEmbed("Stop", "Player is already stopped!");
                await ReplyAsync(embed: alreadyStoppedEmbed);
                return;
            }

            try
            {
                await _lavaNode.LeaveAsync(player.VoiceChannel);
                await player.StopAsync();
                player.Vueue.Clear();
                var embed = await EmbedHandler.CreateBasicEmbed("Stopped player", "Playback has stopped and the queue has been cleared. Bye!");
                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Stop", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Stop", null, ex);
            }
        }
    }
}
