﻿using Discord.Commands;
using TharBot.Handlers;
using Victoria;

namespace TharBot.Commands
{
    public class Queue : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Queue(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        [Command("Queue")]
        [Alias("Now", "Nowplaying", "np")]
        [Summary("Shows the current music queue.\n" +
            "**USAGE:** th.queue")]
        public async Task QueueAsync()
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                var noPlayerEmbed = await EmbedHandler.CreateUserErrorEmbed("Queue", "Could not acquire player.\n" +
                    "Are you sure the bot is active right now? Try using the Play command to start the player.");
                await ReplyAsync(embed: noPlayerEmbed);
            }
            else
            {
                try
                {
                    var player = _lavaNode.GetPlayer(Context.Guild);
                    var track = player.Track;
                    if (track == null)
                    {
                        var noQueueEmbed = await EmbedHandler.CreateUserErrorEmbed("Queue", "Could not acquire queue.\n" +
                            "There's no songs in queue right now, add some with the play command!");

                        await ReplyAsync(embed: noQueueEmbed);
                    }
                    else
                    {
                        var embed = await EmbedHandler.CreateMusicEmbedBuilder("Now Playing:", $"{track.Title} - {track.Duration:%h\\:mm\\:ss} / {track.Position:%h\\:mm\\:ss}\n{track.Url}", player, true, false);

                        await ReplyAsync(embed: embed.Build());
                    }
                }
                catch (Exception ex)
                {
                    var exEmbed = await EmbedHandler.CreateErrorEmbed("Queue", ex.Message);
                    await ReplyAsync(embed: exEmbed);
                    await LoggingHandler.LogCriticalAsync("COMND: Queue", null, ex);
                }
            }
        }
    }
}
