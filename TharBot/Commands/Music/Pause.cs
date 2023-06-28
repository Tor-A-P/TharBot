using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;
using Victoria;
using Victoria.Node;
using Victoria.Player;

namespace TharBot.Commands
{
    public class Pause : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Pause(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        [Command("Pause")]
        [Summary("Pauses the playback of the current song, to be resumed later with th.resume\n" +
            "**USAGE:** th.pause")]
        [Remarks("Music")]
        public async Task PauseAsync()
        {
            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
            {
                var notPlayingEmbed = await EmbedHandler.CreateUserErrorEmbed("Pause", "I'm not connected to a voice channel!");
                await ReplyAsync(embed: notPlayingEmbed);
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                var alreadyStoppedEmbed = await EmbedHandler.CreateUserErrorEmbed("Pause", "I'm not playing anything!");
                await ReplyAsync(embed: alreadyStoppedEmbed);
                return;
            }

            try
            {
                await player.PauseAsync();

                string shortTitle = player.Track.Title.Length > 40 ? player.Track.Title.Substring(0, 40) + "..." : player.Track.Title;
                var embed = await EmbedHandler.CreateMusicEmbedBuilder("Paused song", $"Paused {shortTitle} at {player.Track.Position:%h\\:mm\\:ss}", player, false);

                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Pause", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Pause", null, ex);
            }
            //if (!_lavaNode.HasPlayer(Context.Guild))
            //{
            //    var noPlayerEmbed = await EmbedHandler.CreateUserErrorEmbed("Pause", $"Could not acquire player.\n" +
            //        $"Are you sure the bot is active right now? Try using the Play command to start the player.");
            //    await ReplyAsync(embed: noPlayerEmbed);
            //    return;
            //}

            //var player = _lavaNode.GetPlayer(Context.Guild);

            //var commandUser = Context.User as SocketGuildUser;
            //var bot = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;

            //if (commandUser.VoiceChannel == null || commandUser.VoiceChannel != bot.VoiceChannel)
            //{
            //    var notVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Pause", "You must be connected to the same voice channel as the bot!");
            //    await ReplyAsync(embed: notVCEmbed);
            //}
            //else if (player.PlayerState is not PlayerState.Playing)
            //{
            //    var notPlayingEmbed = await EmbedHandler.CreateUserErrorEmbed("Pause", "The player is not currently playing anything, try playing or resuming something before pausing");
            //    await ReplyAsync(embed: notPlayingEmbed);
            //}
            //else
            //{
            //    try
            //    {
            //        await player.PauseAsync();

            //        string shortTitle = player.Track.Title.Length > 40 ? player.Track.Title.Substring(0, 40) + "..." : player.Track.Title;
            //        var embed = await EmbedHandler.CreateMusicEmbedBuilder("Paused song", $"Paused {shortTitle} at {player.Track.Position:%h\\:mm\\:ss}", player, false);

            //        await ReplyAsync(embed: embed.Build());
            //    }
            //    catch (Exception ex)
            //    {
            //        var exEmbed = await EmbedHandler.CreateErrorEmbed("Pause", ex.Message);
            //        await ReplyAsync(embed: exEmbed);
            //        await LoggingHandler.LogCriticalAsync("COMND: Pause", null, ex);
            //    }
            //}
        }
    }
}
