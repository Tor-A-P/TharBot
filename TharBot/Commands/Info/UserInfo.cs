using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Whois : ModuleBase<SocketCommandContext>
    {
        [Command("UserInfo")]
        [Alias("whois")]
        [Summary("Shows information about a mentioned user. If no user specified, shows information about current user.\n" +
                "**USAGE:** th.whois, th.whois [@USER_MENTION]")]
        [Remarks("Info")]
        public async Task UserInfoAsync(SocketGuildUser? user = null)
        {
            try
            {
                if (user == null) user = Context.Guild.GetUser(Context.User.Id);
                var roles = "";
                foreach (var role in user.Roles.OrderByDescending(x => x.Position))
                {
                    if (role.Name == "@everyone") continue;
                    roles += role.Mention + ", ";
                }
                if (roles != "") roles = roles.Remove(roles.Length - 2, 2);
                else roles = "None";

                var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder($"Info for {user.Username}#{user.Discriminator}");

                var embed = embedBuilder.AddField("ID", user.Id, true)
                    .AddField("Nickname", user.DisplayName, true)
                    .AddField("Account Created", TimestampTag.FromDateTimeOffset(user.CreatedAt))
                    .AddField("Join date", TimestampTag.FromDateTimeOffset((DateTimeOffset)user.JoinedAt))
                    .AddField("Number of guilds shared with TharBot", Context.User.MutualGuilds.Count)
                    .AddField("Roles", roles)
                    .WithThumbnailUrl(user.GetAvatarUrl(Discord.ImageFormat.Auto, 2048) ?? user.GetDefaultAvatarUrl())
                    .WithCurrentTimestamp()
                    .Build();

                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Whois", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Whois", null, ex);
            }
        }
    }
}
