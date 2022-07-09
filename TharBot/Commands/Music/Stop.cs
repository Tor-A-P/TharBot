using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;
using Victoria;
using Victoria.Enums;

namespace TharBot.Commands
{
    public class Stop : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Stop(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        [Command("Stop")]
        [Summary("Causes the bot to stop the player, clear the queue, and leave the voice channel\n" +
            "**USAGE:** th.stop")]
        [Remarks("Music")]
        public async Task StopAsync()
        {
            var commandUser = Context.User as SocketGuildUser;
            var bot = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;

            if (commandUser.VoiceChannel == null || commandUser.VoiceChannel != bot.VoiceChannel)
            {
                var notVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Stop", "You must be connected to the same voice channel as the bot!");
                await ReplyAsync(embed: notVCEmbed);
                return;
            }
            else if (!_lavaNode.HasPlayer(Context.Guild))
            {
                var noPlayerEmbed = await EmbedHandler.CreateUserErrorEmbed("Stop", $"Could not acquire player.\n" +
                        $"Are you sure the bot is active right now? Try using the Play command to start the player.");
                await ReplyAsync(embed: noPlayerEmbed);
                return;
            }

            try
            {
                var player = _lavaNode.GetPlayer(Context.Guild);

                if (player.PlayerState is PlayerState.Playing && commandUser.VoiceChannel != null)
                {
                    player.Queue.Clear();
                    await player.StopAsync();

                    var embed = await EmbedHandler.CreateBasicEmbed("Stopped player", "Playback has stopped and the queue has been cleared. Bye!");

                    await _lavaNode.LeaveAsync(commandUser.VoiceChannel);
                    await ReplyAsync(embed: embed);
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Stop", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Stop", null, ex);
            }
        }
    }
}
