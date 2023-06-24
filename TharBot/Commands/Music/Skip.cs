using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;
using Victoria.Node;
using Victoria.Player;

namespace TharBot.Commands
{
    public class Skip : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Skip(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        [Command("Skip")]
        [Summary("Skips the current song, or a specified song by number in queue\n" +
            "**USAGE:** -skip, -skip [NUMBER]\n" +
            "**EXAMPLES:** -skip, -skip 1, -skip 4")]
        [Remarks("Music")]
        public async Task SkipAsync(int position = 0)
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                var notPlayingEmbed = await EmbedHandler.CreateUserErrorEmbed("Skip", "I'm not connected to a voice channel!");
                await ReplyAsync(embed: notPlayingEmbed);
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                var alreadyStoppedEmbed = await EmbedHandler.CreateUserErrorEmbed("Skip", "I'm not playing anything!");
                await ReplyAsync(embed: alreadyStoppedEmbed);
                return;
            }

            var commandUser = Context.User as SocketGuildUser;
            var bot = Context.Guild.GetUser(Context.Client.CurrentUser.Id);

            if (commandUser.VoiceChannel != bot.VoiceChannel)
            {
                var wrongVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Skip", "You must be connected to the same voice channel as the bot!");
                await ReplyAsync(embed: wrongVCEmbed);
                return;
            }

            try
            {
                var (skipped, currenTrack) = await player.SkipAsync();
                var embedBuilder = await EmbedHandler.CreateMusicEmbedBuilder("Skipped song!", $"Skipped {skipped.Title}\n**NOW PLAYING:**\n\n{currenTrack.Title} / {currenTrack.Duration:%h\\:mm\\:ss}\n{currenTrack.Url}", player);
                await ReplyAsync(embed: embedBuilder.Build());
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Skip", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Skip", null, ex);
            }
        }
    }
}
