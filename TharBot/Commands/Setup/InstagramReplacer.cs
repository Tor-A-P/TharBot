using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TharBot.DBModels;
using TharBot.Handlers;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

namespace TharBot.Commands
{
    public class InstagramReplacer : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;

        public InstagramReplacer(IConfiguration configuration)
        {
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", configuration);
        }

        [Command("InstagramReplacer")]
        [Alias("igr")]
        [Summary("Toggles whether or not the bot should reply to instagram links with a link free of trackers and using ddinstagram, or delete the message containing the link entirely\n" +
            "If a message contains more text than only the instagram link, the delete option will function like the reply option instead.\n" +
            "The reposted link comes with a reaction that the original poster can react to, which will delete the repost.\n" +
            "**USAGE:** th.instagramreplacer [OPTION]\n" +
            "**EXAMPLES:** th.instagramreplacer reply, th.igr delete, th.igr off")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task PrefixAsync([Remainder] string? instaOption = null)
        {
            if (Context.User.IsBot) return;

            var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
            if (serverSettings == null) return;
            if (instaOption == null)
            {
                var noOptionEmbed = await EmbedHandler.CreateUserErrorEmbed("InstagramReplacer", "No option entered! Please use the command with one of the following options: reply, delete, off.\n" +
                    $"**EXAMPLE:** {serverSettings.Prefix}instagramreplacer off");
                await ReplyAsync(embed: noOptionEmbed);
                return;
            }

            instaOption = instaOption.ToLower();

            if (instaOption == "reply")
            {
                var embed = await EmbedHandler.CreateBasicEmbed("InstagramReplacer", "The bot will now reply to instagram links with a fixed link.\n" +
                    "The original poster can react with the added reaction to delete the repost.");
                serverSettings.ReplaceInstagramLinks = "reply";
                await ReplyAsync(embed: embed);
            }
            else if (instaOption == "delete")
            {
                var embed = await EmbedHandler.CreateBasicEmbed("InstagramReplacer", "The bot will now delete the original message, and post a fixed instagram link.\n" +
                    "If the message contains more than just a instagram link, it will not delete it, and simply reply with a fixed link.\n" +
                    "The original poster can react with the added reaction to delete the repost.");
                serverSettings.ReplaceInstagramLinks = "delete";
                await ReplyAsync(embed: embed);
            }
            else if (instaOption == "off")
            {
                var embed = await EmbedHandler.CreateBasicEmbed("InstagramReplacer", "The bot will now ignore instagram links and not do anything about them.");
                serverSettings.ReplaceInstagramLinks = "off";
                await ReplyAsync(embed: embed);
            }
            else
            {
                var invalidOptionEmbed = await EmbedHandler.CreateUserErrorEmbed("InstagramReplacer", $"\"{instaOption}\" is not a valid option! Please use the command with one of the following options: reply, delete, off.\n" +
                    $"**EXAMPLE:** {serverSettings.Prefix}instagramreplacer off");
                await ReplyAsync(embed: invalidOptionEmbed);
                return;
            }

            var update = Builders<ServerSpecifics>.Update.Set(x => x.ReplaceInstagramLinks, serverSettings.ReplaceInstagramLinks);
            await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
        }
    }
}
