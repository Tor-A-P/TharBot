using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;
using Victoria;
using Victoria.Node;

namespace TharBot.Commands
{
    public class Resume : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Resume(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        [Command("Resume")]
        [Summary("Resumes playback for a track paused with th.pause\n" +
            "**USAGE:** th.resume")]
        [Remarks("Music")]
        public async Task ResumeAsync()
        {
            //if (!_lavaNode.HasPlayer(Context.Guild))
            //{
            //    var noPlayerEmbed = await EmbedHandler.CreateUserErrorEmbed("Resume", $"Could not acquire player.\n" +
            //            $"Are you sure the bot is active right now? Try using the Play command to start the player.");
            //    await ReplyAsync(embed: noPlayerEmbed);
            //    return;
            //}

            //var player = _lavaNode.GetPlayer(Context.Guild);

            //var commandUser = Context.User as SocketGuildUser;
            //var bot = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;

            //if (commandUser.VoiceChannel == null || commandUser.VoiceChannel != bot.VoiceChannel)
            //{
            //    var notVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Resume", "You must be connected to the same voice channel as the bot!");
            //    await ReplyAsync(embed: notVCEmbed);
            //}
            //else if (player.PlayerState is not PlayerState.Paused)
            //{
            //    var notPlayingEmbed = await EmbedHandler.CreateUserErrorEmbed("Resume", "The player is not currently playing anything, try pausing something before resuming");
            //    await ReplyAsync(embed: notPlayingEmbed);
            //}
            //else
            //{
            //    try
            //    {
            //        await player.ResumeAsync();

            //        string shortTitle = player.Track.Title.Length > 40 ? player.Track.Title.Substring(0, 40) + "..." : player.Track.Title;

            //        var embed = await EmbedHandler.CreateMusicEmbedBuilder("Resumed song", $"Resumed {shortTitle} at {player.Track.Position:%h\\:mm\\:ss}", player, false);

            //        await ReplyAsync(embed: embed.Build());
            //    }
            //    catch (Exception ex)
            //    {
            //        var exEmbed = await EmbedHandler.CreateErrorEmbed("Pause", ex.Message);
            //        await ReplyAsync(embed: exEmbed);
            //        await LoggingHandler.LogCriticalAsync("COMND: Resume", null, ex);
            //    }
            //}
        }
    }
}
