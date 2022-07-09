using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;
using Victoria;

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
            var commandUser = Context.User as SocketGuildUser;
            var bot = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;

            if (commandUser.VoiceChannel == null || commandUser.VoiceChannel != bot.VoiceChannel)
            {
                var notVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Skip", "You must be connected to the same voice channel as the bot!");
                await ReplyAsync(embed: notVCEmbed);
                return;
            }
            else if (!_lavaNode.HasPlayer(Context.Guild))
            {
                var noPlayerEmbed = await EmbedHandler.CreateUserErrorEmbed("Skip", $"Could not acquire player.\n" +
                        $"Are you sure the bot is active right now? Try using the Play command to start the player.");
                await ReplyAsync(embed: noPlayerEmbed);
            }

            if (_lavaNode.HasPlayer(Context.Guild) && commandUser.VoiceChannel != null)
            {
                try
                {
                    var player = _lavaNode.GetPlayer(Context.Guild);

                    if (player.Queue.Count > 0)
                    {
                        if (position < 1)
                        {
                            var skippedTrack = player.Track;

                            var skippedEmbed = await EmbedHandler.CreateMusicEmbedBuilder("Skipped song:", $"{skippedTrack.Title}\n{skippedTrack.Url}", player, false);

                            await player.SkipAsync();

                            await ReplyAsync(embed: skippedEmbed.Build());

                            var nextEmbed = await EmbedHandler.CreateMusicEmbedBuilder("Now Playing:", $"{player.Track.Title} / {player.Track.Duration}\n{player.Track.Url}", player, true, false);
                            await ReplyAsync(embed: nextEmbed.Build());
                        }
                        else if (player.Queue.Count >= position)
                        {
                            var removedTrack = player.Queue.RemoveAt(position - 1);

                            var embed = await EmbedHandler.CreateMusicEmbedBuilder("Removed song from queue:", $"{removedTrack.Title}\n{removedTrack.Url}", player);
                            embed = embed.WithThumbnailUrl(await removedTrack.FetchArtworkAsync());

                            await ReplyAsync(embed: embed.Build());
                        }
                        else
                        {
                            var noPosEmbed = await EmbedHandler.CreateUserErrorEmbed("Skip", $"Cannot remove song number {position}, as there's only {player.Queue.Count} songs in the queue!");
                            await ReplyAsync(embed: noPosEmbed);
                        }

                    }
                    else
                    {
                        var noQueueEmbed = await EmbedHandler.CreateUserErrorEmbed("Skip", "There are no songs in the queue, use the stop command instead!");
                        await ReplyAsync(embed: noQueueEmbed);
                    }
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
}
