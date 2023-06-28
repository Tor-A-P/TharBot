using Albatross.Expression.Operations;
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
            "**USAGE:** th.play [YOUTUBE_OR_SOUNDCLOUD_LINK], th.play [SEARCH_TERM]\n" +
            "**EXAMPLES:** th.play https://www.youtube.com/watch?v=dQw4w9WgXcQ, th.play https://soundcloud.com/user-221655804-383124150/agent-yoru-o-yuku-the-idolmster-siivagunner, th.play rickroll")]
        [Remarks("Music")]
        public async Task PlayAsync([Remainder] string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                var noSearchEmbed = await EmbedHandler.CreateUserErrorEmbed("Play", "Please provide any search terms");
                await ReplyAsync(embed: noSearchEmbed);
                return;
            }

            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                var voiceState = Context.User as IVoiceState;
                if (voiceState?.VoiceChannel == null)
                {
                    var noVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Play", "You must be connected to a voice channel!");
                    await ReplyAsync(embed: noVCEmbed);
                    return;
                }

                try
                {
                    player = await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                }
                catch (Exception ex)
                {
                    var exEmbed = await EmbedHandler.CreateErrorEmbed("Play", ex.Message);
                    await ReplyAsync(embed: exEmbed);
                    await LoggingHandler.LogCriticalAsync("COMND: Play", null, ex);
                }
            }

            var commandUser = Context.User as SocketGuildUser;
            var bot = Context.Guild.GetUser(Context.Client.CurrentUser.Id);

            await Task.Delay(1000);
            if (commandUser.VoiceChannel != bot.VoiceChannel)
            {
                var wrongVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Play", "You must be connected to the same voice channel as the bot!");
                await ReplyAsync(embed: wrongVCEmbed);
                return;
            }

            var searchResponse = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(search, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, search);
            if (searchResponse.Status is SearchStatus.LoadFailed or SearchStatus.NoMatches)
            {
                var notFoundEmbed = await EmbedHandler.CreateErrorEmbed("Play", $"Unable to find anything for \"{search}\"!");
                await ReplyAsync(embed: notFoundEmbed);
                return;
            }

            if (!string.IsNullOrWhiteSpace(searchResponse.Playlist.Name))
            {
                try
                {
                    player.Vueue.Enqueue(searchResponse.Tracks);
                    if (!(player.Track != null && (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)))
                    {
                        player.Vueue.TryDequeue(out var lavaTrack);
                        await player.PlayAsync(lavaTrack);
                    }
                    var embed = await EmbedHandler.CreateMusicEmbedBuilder("Added to queue:", $"{searchResponse.Tracks.First().Title} / {searchResponse.Tracks.First().Duration:%h\\:mm\\:ss}\n{searchResponse.Tracks.First().Url}\n\nAnd {searchResponse.Tracks.Count - 1} more songs.", player);
                    await ReplyAsync(embed: embed);
                }
                catch (Exception ex)
                {
                    var exEmbed = await EmbedHandler.CreateErrorEmbed("Play", "Sorry, that song created an error, I'll look into it but in the meantime try another song!");
                    await ReplyAsync(embed: exEmbed);
                    await LoggingHandler.LogCriticalAsync("COMND: Play", null, ex);
                }
            }
            else
            {
                try
                {
                    if (player.Track != null && (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused))
                    {
                        var track = searchResponse.Tracks.FirstOrDefault();
                        player.Vueue.Enqueue(track);

                        var embed = await EmbedHandler.CreateMusicEmbedBuilder("Added to queue:", $"{searchResponse.Tracks.First().Title} / {searchResponse.Tracks.First().Duration:%h\\:mm\\:ss}\n{searchResponse.Tracks.First().Url}", player);
                        await ReplyAsync(embed: embed);
                    }
                    else
                    {
                        var track = searchResponse.Tracks.FirstOrDefault();
                        player.Vueue.Enqueue(track);

                        if (player.PlayerState is PlayerState.Playing or PlayerState.Paused)
                        {
                            return;
                        }

                        player.Vueue.TryDequeue(out var lavaTrack);
                        await player.PlayAsync(lavaTrack);

                        var embed = await EmbedHandler.CreateMusicEmbedBuilder("Now Playing:", $"{track.Title} / {track.Duration:%h\\:mm\\:ss}\n{track.Url}", player);
                        await ReplyAsync(embed: embed);
                    }
                }
                catch (Exception ex)
                {
                    var exEmbed = await EmbedHandler.CreateErrorEmbed("Play", "Sorry, that song created an error, I'll look into it but in the meantime try another song!");
                    await ReplyAsync(embed: exEmbed);
                    await LoggingHandler.LogCriticalAsync("COMND: Play", null, ex);
                }
            }
        }
    }
}
