using Discord.Commands;
using TharBot.Handlers;
using Victoria.Node;
using Victoria.Player;

namespace TharBot.Commands
{
    public class Shuffle : ModuleBase<SocketCommandContext>
    {
        private LavaNode _lavaNode;
        private static Random rng = new();

        public Shuffle(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        [Command("Shuffle")]
        [Alias("Randomize")]
        [Summary("Shuffles the current queue, putting the songs in a random order.\n" +
            "**USAGE:** th.shuffle")]
        public async Task ShuffleAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                var notPlayingEmbed = await EmbedHandler.CreateUserErrorEmbed("Shuffle", "I'm not connected to a voice channel!");
                await ReplyAsync(embed: notPlayingEmbed);
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                var alreadyStoppedEmbed = await EmbedHandler.CreateUserErrorEmbed("Shuffle", "I'm not playing anything!");
                await ReplyAsync(embed: alreadyStoppedEmbed);
                return;
            }

            if (player.Vueue.Count < 2)
            {
                var noQueueEmbed = await EmbedHandler.CreateUserErrorEmbed("Shuffle", "There is either 0 or 1 songs in the queue, shuffling will do nothing!");
                await ReplyAsync(embed: noQueueEmbed);
            }
            else
            {
                try
                {
                    var shuffledQueue = player.Vueue.OrderBy(x => rng.Next()).ToList();
                    player.Vueue.Clear();

                    foreach (var track in shuffledQueue)
                    {
                        player.Vueue.Enqueue(track);
                    }

                    var embed = await EmbedHandler.CreateMusicEmbedBuilder("Queue shuffled!", "Queue has been shuffled successfully.", player);
                    await ReplyAsync(embed: embed.Build());
                }
                catch (Exception ex)
                {
                    var exEmbed = await EmbedHandler.CreateErrorEmbed("Shuffle", ex.Message);
                    await ReplyAsync(embed: exEmbed);
                    await LoggingHandler.LogCriticalAsync("COMND: Shuffle", null, ex);
                }
            }
        }
    }
}
