//using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
//using TharBot.Handlers;
//using Victoria;
//using Victoria.Node;
//using Victoria.Player;

//namespace TharBot.Commands
//{
//    public class Resume : ModuleBase<SocketCommandContext>
//    {
//        private readonly LavaNode _lavaNode;

//        public Resume(LavaNode lavaNode)
//            => _lavaNode = lavaNode;

//        [Command("Resume")]
//        [Summary("Resumes playback for a track paused with th.pause\n" +
//            "**USAGE:** th.resume")]
//        [Remarks("Music")]
//        public async Task ResumeAsync()
//        {
//            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
//            {
//                var notPlayingEmbed = await EmbedHandler.CreateUserErrorEmbed("Resume", "I'm not connected to a voice channel!");
//                await ReplyAsync(embed: notPlayingEmbed);
//                return;
//            }

//            if (player.PlayerState != PlayerState.Paused)
//            {
//                var alreadyStoppedEmbed = await EmbedHandler.CreateUserErrorEmbed("Resume", "I'm not paused, play something with the play command instead!");
//                await ReplyAsync(embed: alreadyStoppedEmbed);
//                return;
//            }

//            try
//            {
//                await player.ResumeAsync();

//                string shortTitle = player.Track.Title.Length > 40 ? player.Track.Title.Substring(0, 40) + "..." : player.Track.Title;
//                var embed = await EmbedHandler.CreateMusicEmbedBuilder("Resumed song", $"Resumed {shortTitle} from {player.Track.Position:%h\\:mm\\:ss}", player, false);

//                await ReplyAsync(embed: embed);
//            }
//            catch (Exception ex)
//            {
//                var exEmbed = await EmbedHandler.CreateErrorEmbed("Resume", ex.Message);
//                await ReplyAsync(embed: exEmbed);
//                await LoggingHandler.LogCriticalAsync("COMND: Resume", null, ex);
//            }
//        }
//    }
//}
