//using Albatross.Expression.Operations;
//using Discord;
//using Discord.Commands;
//using Discord.WebSocket;
//using TharBot.Handlers;
//using Victoria;
//using Victoria.Node;
//using Victoria.Player;
//using Victoria.Responses.Search;

//namespace TharBot.Commands
//{
//    public class Join : ModuleBase<SocketCommandContext>
//    {
//        private readonly LavaNode _lavaNode;

//        public Join(LavaNode lavaNode)
//            => _lavaNode = lavaNode;

//        [Command("Join")]
//        [Summary("Forces the bot to join the voice channel you're in." +
//            "**USAGE:** th.join")]
//        [Remarks("Music")]
//        public async Task JoinAsync()
//        {
//            var voiceState = Context.User as IVoiceState;
//            if (voiceState?.VoiceChannel == null)
//            {
//                await ReplyAsync("You must be connected to a voice channel!");
//                return;
//            }

//            try
//            {
//                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
//                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
//            }
//            catch (Exception exception)
//            {
//                await ReplyAsync(exception.Message);
//            }
//        }
//    }
//}
