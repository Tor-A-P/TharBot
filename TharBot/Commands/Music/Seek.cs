using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;
using Victoria;

namespace TharBot.Commands
{
    public class Seek : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Seek(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        [Command("Seek")]
        [Summary("Seeks within the current video to the specified timestamp.\n" +
            "**USAGE:** th.seek [TIMESTAMP_HH:MM:SS], th.seek [TIMESTAMP_MM:SS]\n" +
            "**EXAMPLES:** th.seek 01:33:37, th.seek 42:69, th.seek 4:20")]
        [Remarks("Music")]
        public async Task SeekAsync(string position)
        {
            var commandUser = Context.User as SocketGuildUser;
            var bot = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;

            if (commandUser.VoiceChannel == null || commandUser.VoiceChannel != bot.VoiceChannel)
            {
                var notVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Seek", "You must be connected to the same voice channel as the bot!");
                await ReplyAsync(embed: notVCEmbed);
                return;
            }
            else if (!_lavaNode.HasPlayer(Context.Guild))
            {
                var noPlayerEmbed = await EmbedHandler.CreateUserErrorEmbed("Seek", $"Could not acquire player.\n" +
                        $"Are you sure the bot is active right now? Try using the Play command to start the player.");
                await ReplyAsync(embed: noPlayerEmbed);
                return;
            }
            
            try
            {
                var player = _lavaNode.GetPlayer(Context.Guild);

                if (!player.Track.CanSeek)
                {
                    var noSeekingEmbed = await EmbedHandler.CreateErrorEmbed("Seek", "Cannot seek on this track! Try another one.");
                    await ReplyAsync(embed: noSeekingEmbed);
                }
                else
                {
                    if (TimeSpan.TryParseExact(position, @"hh\:mm\:ss", null, out TimeSpan timeSpan))
                    {
                        if (timeSpan > TimeSpan.Zero && timeSpan < player.Track.Duration)
                        {
                            await player.SeekAsync(timeSpan);
                            var embed = await EmbedHandler.CreateMusicEmbedBuilder("Seek", $"Jumped to position {timeSpan:%h\\:mm\\:ss} of the current song!", player, false);
                            await ReplyAsync(embed: embed.Build());
                        }
                        else
                        {
                            var timeSpanOutOfRangeEmbed = await EmbedHandler.CreateUserErrorEmbed("Seek",
                                $"The timespan {timeSpan:%h\\:mm\\:ss} is out of range for this track, as it is only {player.Track.Duration:%h\\:mm\\:ss} long!");
                            await ReplyAsync(embed: timeSpanOutOfRangeEmbed);
                        } 
                    }
                    else if (TimeSpan.TryParseExact(position, @"mm\:ss", null, out TimeSpan timeSpanShort))
                    {
                        if (timeSpanShort > TimeSpan.Zero && timeSpanShort < player.Track.Duration)
                        {
                            await player.SeekAsync(timeSpanShort);
                            var embed = await EmbedHandler.CreateMusicEmbedBuilder("Seek", $"Jumped to position {timeSpanShort:mm\\:ss} of the current song!", player, false);
                            await ReplyAsync(embed: embed.Build());
                        }
                        else
                        {
                            var timeSpanOutOfRangeEmbed = await EmbedHandler.CreateUserErrorEmbed("Seek",
                                $"The timespan {timeSpanShort:mm\\:ss} is out of range for this track, as it is only {player.Track.Duration:mm\\:ss} long!");
                            await ReplyAsync(embed: timeSpanOutOfRangeEmbed);
                        }
                    }
                    else
                    {
                        var wrongFormatEmbed = await EmbedHandler.CreateErrorEmbed("Seek", "Could not parse the time provided, use format \"hh:mm:ss\" or \"mm:ss\" (mm and ss cannot exceed 60)");
                        await ReplyAsync(embed: wrongFormatEmbed);
                    }
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Seek", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Seek", null, ex);
            }
        }
    }
}
