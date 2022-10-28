using Discord;
using Discord.Commands;
using TharBot.Handlers;


namespace TharBot.Commands
{
    public class Ban : ModuleBase<SocketCommandContext>
    {
        [Command("Ban")]
        [Alias("b")]
        [Summary("Bans a user from the guild and removes their last 24 hours worth of messages.\n" +
                "**USAGE:** th.ban [USER_MENTION] [OPTIONAL_REASON]")]
        [Remarks("Admin")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task BanMemberAsync(IGuildUser? user = null, string? reason = null)
        {
            if (Context.User.IsBot) return;

            if (user == null)
            {
                var noUserEmbed = await EmbedHandler.CreateUserErrorEmbed("Ban", "No user specified, please mention a user to ban!");
                await ReplyAsync(embed: noUserEmbed);
                return;
            }

            try
            {
                await user.BanAsync(1, reason);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Ban", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Ban", null, ex);
            }

            var embed = new EmbedBuilder()
                .WithTitle("User Banned")
                .AddField("Banned", user.Mention)
                .AddField("Reason", reason ?? "No reason given")
                .AddField("Banned by", Context.User.Mention)
                .WithColor(new Color(199, 10, 0))
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
