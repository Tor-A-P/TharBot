using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;
using Victoria;

namespace TharBot.Commands
{
    public class Volume : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Volume(LavaNode lavaNode)
            => _lavaNode = lavaNode;

        [Command("Volume")]
        [Alias("Vol")]
        [Summary("Adjusts the volume of the bot, between 1 - 150.\n" +
            "**USAGE:** th.volume [VOLUME_VALUE]\n" +
            "**EXAMPLE:** th.volume 42, th.vol 69")]
        [Remarks("Music")]
        public async Task VolumeAsync(int volume)
        {
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                var noPlayerEmbed = await EmbedHandler.CreateUserErrorEmbed("Volume", $"Could not acquire player.\n" +
                    $"Are you sure the bot is active right now? Try using the Play command to start the player.");
                await ReplyAsync(embed: noPlayerEmbed);
                return;
            }

            var commandUser = Context.User as SocketGuildUser;
            var bot = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as SocketGuildUser;

            if (commandUser.VoiceChannel == null || commandUser.VoiceChannel != bot.VoiceChannel)
            {
                var notVCEmbed = await EmbedHandler.CreateUserErrorEmbed("Volume", "You must be connected to the same voice channel as the bot!");
                await ReplyAsync(embed: notVCEmbed);
            }
            else if (volume > 150 || volume <= 0)
            {
                var wrongVolumeEmbed = await EmbedHandler.CreateUserErrorEmbed("Volume", "Volume must be between 1 and 150!");
                await ReplyAsync(embed: wrongVolumeEmbed);
            }
            else
            {
                try
                {
                    var player = _lavaNode.GetPlayer(Context.Guild);

                    await player.UpdateVolumeAsync((ushort)volume);
                    await ReplyAsync($"Volume has been set to {volume}");
                }
                catch (Exception ex)
                {
                    var exEmbed = await EmbedHandler.CreateErrorEmbed("Volume", ex.Message);
                    await ReplyAsync(embed: exEmbed);
                    await LoggingHandler.LogCriticalAsync("COMND: Volume", null, ex);
                }
            }
        }
    }
}
