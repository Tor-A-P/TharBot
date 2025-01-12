//using Discord.Commands;
//using TharBot.Handlers;
//using Victoria.Node;
//using Victoria.Player;

//namespace TharBot.Commands
//{
//    public class Queue : ModuleBase<SocketCommandContext>
//    {
//        private readonly LavaNode _lavaNode;

//        public Queue(LavaNode lavaNode)
//            => _lavaNode = lavaNode;

//        [Command("Queue")]
//        [Alias("Now", "Nowplaying", "np")]
//        [Summary("Shows the current music queue.\n" +
//            "**USAGE:** th.queue")]
//        [Remarks("Music")]
//        public async Task QueueAsync()
//        {
//            if (!_lavaNode.TryGetPlayer(Context.Guild, out var player))
//            {
//                var notPlayingEmbed = await EmbedHandler.CreateUserErrorEmbed("Queue", "I'm not connected to any voice channel!");
//                await ReplyAsync(embed: notPlayingEmbed);
//                return;
//            }

//            if (player.PlayerState != PlayerState.Playing)
//            {
//                var notPlayingEmbed = await EmbedHandler.CreateUserErrorEmbed("Queue", "I'm not playing anything!");
//                await ReplyAsync(embed: notPlayingEmbed);
//                return;
//            }

//            var embed = await EmbedHandler.CreateMusicEmbedBuilder("Now Playing:", $"{player.Track.Title} - {player.Track.Position:%h\\:mm\\:ss} / {player.Track.Duration:%h\\:mm\\:ss}\n{player.Track.Url}", player, true, false);

//            await ReplyAsync(embed: embed);
//        }
//    }
//}
