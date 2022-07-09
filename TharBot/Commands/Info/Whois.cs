using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Whois : ModuleBase<SocketCommandContext>
    {
        [Command("Whois")]
        [Alias("Info")]
        [Summary("Shows information about a mentioned user. If no user specified, shows information about current user.\n" +
                "**USAGE:** th.whois, th.whois [@USER_MENTION]")]
        [Remarks("Info")]
        public async Task UserInfoAsync(SocketUser? user = null)
        {
            if (user == null) user = Context.User;

            var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder($"Info for {user.Username}#{user.Discriminator}");

            var embed = embedBuilder.AddField("ID", user.Id, true)
                .AddField("Created at", user.CreatedAt.LocalDateTime)
                .WithThumbnailUrl(user.GetAvatarUrl(Discord.ImageFormat.Auto, 2048) ?? user.GetDefaultAvatarUrl())
                .WithCurrentTimestamp()
                .Build();
                

            await ReplyAsync(embed: embed);
        }
    }
}
