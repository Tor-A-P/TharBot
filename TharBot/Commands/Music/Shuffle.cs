using Discord.Commands;
using TharBot.Handlers;
using Victoria;

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
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                var noPlayerEmbed = await EmbedHandler.CreateUserErrorEmbed("Shuffle", "Could not acquire player.\n" +
                    "Are you sure the bot is active right now? Try using the Play command to start the player.");
                await ReplyAsync(embed: noPlayerEmbed);
            }
            else
            {
                var player = _lavaNode.GetPlayer(Context.Guild);

                if(player.Queue.Count < 2)
                {
                    var noQueueEmbed = await EmbedHandler.CreateUserErrorEmbed("Shuffle", "There is either 0 or 1 songs in the queue, shuffling will do nothing!");
                    await ReplyAsync(embed: noQueueEmbed);
                }
                else
                {
                    try
                    {
                        var shuffledQueue = player.Queue.OrderBy(x => rng.Next()).ToList();
                        player.Queue.Clear();

                        foreach (var track in shuffledQueue)
                        {
                            player.Queue.Enqueue(track);
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
}
