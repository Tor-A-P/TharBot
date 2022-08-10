using Discord;
using Discord.Commands;
using Victoria;
using Victoria.Responses.Search;
using Victoria.Enums;
using TharBot.Handlers;
using Discord.WebSocket;

namespace TharBot.Commands
{
    public class Play : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Play(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        [Command("Play")]
        [Summary("Plays music from a youtube link, or the first video result from a search term.\n" +
            "**USAGE:** th.play [YOUTUBE_LINK], th.play [SEARCH_TERM]\n" +
            "**EXAMPLES:** th.play https://www.youtube.com/watch?v=dQw4w9WgXcQ, th.play https://youtu.be/UPPrz0mVEhU, th.play rickroll")]
        [Remarks("Music")]
        public async Task PlayAsync([Remainder] string search)
        {
            var commandUser = Context.User as SocketGuildUser;
            var bot = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;

            if (commandUser.VoiceChannel == null)
            {
                var notVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Play", "You must be connected a voice channel!");
                await ReplyAsync(embed: notVCEmbed);
                return;
            }
            
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                try
                {
                    await _lavaNode.JoinAsync(commandUser.VoiceChannel, Context.Channel as ITextChannel);
                    await ReplyAsync($"Joined {commandUser.VoiceChannel.Name}!");
                    await Task.Delay(500);
                }
                catch (Exception ex)
                {
                    var exEmbed = await EmbedHandler.CreateErrorEmbed("Play", ex.Message);
                    await ReplyAsync(embed: exEmbed);
                    await LoggingHandler.LogCriticalAsync("COMND: Play", null, ex);
                }
            }
            else
            {
                if (commandUser.VoiceChannel != bot.VoiceChannel)
                {
                    if (bot.VoiceChannel == null)
                    {
                        try
                        {
                            await _lavaNode.JoinAsync(commandUser.VoiceChannel, Context.Channel as ITextChannel);
                            await ReplyAsync($"Joined {commandUser.VoiceChannel.Name}!");
                            await Task.Delay(500);
                        }
                        catch (Exception ex)
                        {
                            var exEmbed = await EmbedHandler.CreateErrorEmbed("Play", ex.Message);
                            await ReplyAsync(embed: exEmbed);
                            await LoggingHandler.LogCriticalAsync("COMND: Play", null, ex);
                        }
                    }
                    else
                    {
                        var wrongVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Play", "You must be connected to the same voice channel as the bot!");
                        await ReplyAsync(embed: wrongVCEmbed);
                        return;
                    }
                }
            }

            try
            {
                var player = _lavaNode.GetPlayer(Context.Guild);

                LavaTrack? track;

                if (search[0] == '<')
                {
                    search = search.Replace("<", "");
                    search = search.Replace(">", "");
                }

                var searchResult = Uri.IsWellFormedUriString(search, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(SearchType.Direct, search)
                    : await _lavaNode.SearchYouTubeAsync(search);

                if (searchResult.Status is SearchStatus.NoMatches)
                {
                    await ReplyAsync($"Sorry, I couldn't find anything for {search}");
                }
                else if (searchResult.Status is SearchStatus.LoadFailed)
                {
                    var loadFailedEmbed = await EmbedHandler.CreateErrorEmbed("Play", "Loading song failed!");
                    await ReplyAsync(embed: loadFailedEmbed);
                }
                else
                {
                    track = searchResult.Tracks.FirstOrDefault();

                    if (player.Track != null && (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused))
                    {
                        player.Queue.Enqueue(track);

                        var embedBuilder = await EmbedHandler.CreateMusicEmbedBuilder("Added to queue:", $"{track.Title} / {track.Duration:%h\\:mm\\:ss}\n{ track.Url}", player);

                        embedBuilder = embedBuilder
                            .WithThumbnailUrl(await track.FetchArtworkAsync());


                        await ReplyAsync(embed: embedBuilder.Build());
                    }
                    else
                    {
                        await player.PlayAsync(track);

                        var embed = await EmbedHandler.CreateMusicEmbedBuilder("Now Playing:", $"{track.Title} / {track.Duration:%h\\:mm\\:ss}\n{track.Url}", player);

                        await ReplyAsync(embed: embed.Build());
                    }
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Play", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Play", null, ex);
            }
        }
    }
}
