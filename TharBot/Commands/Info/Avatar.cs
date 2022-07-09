using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Avatar : ModuleBase<SocketCommandContext>
    {
        [Command("Avatar")]
        [Alias("Pic", "pfp")]
        [Summary("Returns the high-quality version avatar of the user given. If no user specified, returns command user's avatar.\n" +
                "**USAGE:** th.avatar, th.avatar [@USER_MENTION]")]
        [Remarks("Info")]
        public async Task GetUserAvatarAsync(SocketUser? user = null)
        {
            try
            {
                if (user == null) user = Context.User;

                var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder($"Avatar for {user.Username}#{user.Discriminator}");

                var embed = embedBuilder.WithImageUrl(user.GetAvatarUrl(Discord.ImageFormat.Auto, 2048) ?? user.GetDefaultAvatarUrl())
                    .Build();

                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Avatar", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Avatar", null, ex);
            }
        }
    }
}
