using Discord;
using Discord.Commands;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Kick : ModuleBase<SocketCommandContext>
    {
        [Command("Kick")]
        [Alias("k")]
        [Summary("Kicks a user from the channel.\n" +
                "**USAGE:** th.kick [USER_MENTION] [OPTIONAL_REASON]")]
        [Remarks("Admin")]
        [RequireBotPermission(GuildPermission.KickMembers)]
        [RequireUserPermission(GuildPermission.KickMembers, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task KickMemberAsync(IGuildUser? user = null, string? reason = null)
        {
            if (user == null)
            {
                var noUserEmbed = await EmbedHandler.CreateUserErrorEmbed("Kick", "No user specified, please mention a user to kick!");
                await ReplyAsync(embed: noUserEmbed);
                return;
            }

            try
            {
                await user.KickAsync(reason);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Kick", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Kick", null, ex);
            }

            var embed = new EmbedBuilder()
                .WithTitle("User Kicked")
                .AddField("Kicked", user.Mention)
                .AddField("Reason", reason ?? "No reason given")
                .AddField("Kicked by", Context.User.Mention)
                .WithColor(new Color(199, 10, 0))
                .WithCurrentTimestamp()
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
