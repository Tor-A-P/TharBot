using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;
using Victoria;
using Victoria.Node;
using Victoria.Player;
using Victoria.Responses.Search;

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
            if (string.IsNullOrWhiteSpace(search))
            {
                await ReplyAsync("Please provide search terms.");
                return;
            }

            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                var voiceState = Context.User as IVoiceState;
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }

                try
                {
                    player = await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                    await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
                }
                catch (Exception exception)
                {
                    await ReplyAsync(exception.Message);
                }
            }

            var searchResponse = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(search, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, search);
            if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
            {
                await ReplyAsync($"I wasn't able to find anything for `{search}`.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
            {
                player.Vueue.Enqueue(searchResponse.Tracks);
                await ReplyAsync($"Enqueued {searchResponse.Tracks.Count} songs.");
            }
            else
            {
                var track = searchResponse.Tracks.FirstOrDefault();
                player.Vueue.Enqueue(track);

                await ReplyAsync($"Enqueued {track?.Title}");
            }

            if (player.PlayerState is PlayerState.Playing or PlayerState.Paused)
            {
                return;
            }

            player.Vueue.TryDequeue(out var lavaTrack);
            await player.PlayAsync(lavaTrack);

            //var commandUser = Context.User as SocketGuildUser;
            //var bot = Context.Guild.GetUser(Context.Client.CurrentUser.Id);
            //await Task.Delay(250);

            //if (!_lavaNode.HasPlayer(Context.Guild))
            //{
            //    try
            //    {
            //        await _lavaNode.JoinAsync(commandUser.VoiceChannel, Context.Channel as ITextChannel);
            //        await ReplyAsync($"Joined {commandUser.VoiceChannel.Name}!");
            //    }
            //    catch (Exception ex)
            //    {
            //        var exEmbed = await EmbedHandler.CreateErrorEmbed("Play", ex.Message);
            //        await ReplyAsync(embed: exEmbed);
            //        await LoggingHandler.LogCriticalAsync("COMND: Play", null, ex);
            //    }
            //}
            //else
            //{
            //    if (bot.VoiceChannel == null)
            //    {
            //        bot = Context.Guild.GetUser(Context.Client.CurrentUser.Id);
            //        await Task.Delay(500);
            //        if (bot.VoiceChannel == null)
            //        {
            //            try
            //            {
            //                var voiceState = Context.User as IVoiceState;
            //                if (voiceState?.VoiceChannel == null)
            //                {
            //                    var notVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Play", "You must be connected a voice channel!");
            //                    await ReplyAsync(embed: notVCEmbed);
            //                    return;
            //                }

            //                if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            //                {
            //                    player = await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            //                    await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            //                }
            //            }
            //            catch (Exception ex)
            //            {
            //                var exEmbed = await EmbedHandler.CreateErrorEmbed("Play", ex.Message);
            //                await ReplyAsync(embed: exEmbed);
            //                await LoggingHandler.LogCriticalAsync("COMND: Play", null, ex);
            //            }
            //        }
            //    }

            //    bot = Context.Guild.GetUser(Context.Client.CurrentUser.Id);
            //    await Task.Delay(500);

            //    if (commandUser.VoiceChannel != bot.VoiceChannel)
            //    {
            //        var wrongVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Play", "You must be connected to the same voice channel as the bot!");
            //        await ReplyAsync(embed: wrongVCEmbed);
            //        return;
            //    }
            //}

            //try
            //{
            //    var player = _lavaNode.GetPlayer(Context.Guild);

            //    LavaTrack? track;

            //    if (search[0] == '<')
            //    {
            //        search = search.Replace("<", "");
            //        search = search.Replace(">", "");
            //    }

            //    var searchResult = Uri.IsWellFormedUriString(search, UriKind.Absolute) ?
            //        await _lavaNode.SearchAsync(SearchType.Direct, search)
            //        : await _lavaNode.SearchYouTubeAsync(search);

            //    if (searchResult.Status is SearchStatus.NoMatches)
            //    {
            //        await ReplyAsync($"Sorry, I couldn't find anything for {search}");
            //    }
            //    else if (searchResult.Status is SearchStatus.LoadFailed)
            //    {
            //        var loadFailedEmbed = await EmbedHandler.CreateErrorEmbed("Play", "Loading song failed!");
            //        await ReplyAsync(embed: loadFailedEmbed);
            //    }
            //    else
            //    {
            //        track = searchResult.Tracks.FirstOrDefault();

            //        if (player.Track != null && (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused))
            //        {
            //            player.Queue.Enqueue(track);

            //            var embedBuilder = await EmbedHandler.CreateMusicEmbedBuilder("Added to queue:", $"{track.Title} / {track.Duration:%h\\:mm\\:ss}\n{track.Url}", player);

            //            embedBuilder = embedBuilder
            //                .WithThumbnailUrl(await track.FetchArtworkAsync());


            //            await ReplyAsync(embed: embedBuilder.Build());
            //        }
            //        else
            //        {
            //            await player.PlayAsync(track);

            //            var embed = await EmbedHandler.CreateMusicEmbedBuilder("Now Playing:", $"{track.Title} / {track.Duration:%h\\:mm\\:ss}\n{track.Url}", player);

            //            await ReplyAsync(embed: embed.Build());
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    var exEmbed = await EmbedHandler.CreateErrorEmbed("Play", ex.Message);
            //    await ReplyAsync(embed: exEmbed);
            //    await LoggingHandler.LogCriticalAsync("COMND: Play", null, ex);
            //}
        }
    }
}
