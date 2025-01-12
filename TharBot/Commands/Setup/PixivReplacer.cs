using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TharBot.DBModels;
using TharBot.Handlers;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

namespace TharBot.Commands
{
    public class PixivReplacer : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;

        public PixivReplacer(IConfiguration configuration)
        {
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", configuration);
        }

        [Command("PixivReplacer")]
        [Alias("pr")]
        [Summary("Toggles whether or not the bot should reply to pixiv links with a link to phixiv instead for embed purposes, or delete the message containing the link entirely\n" +
            "If a message contains more text than only the pixiv link, the delete option will function like the reply option instead.\n" +
            "The reposted link comes with a reaction that the original poster can react to, which will delete the repost.\n" +
            "**USAGE:** th.pixivreplacer [OPTION]\n" +
            "**EXAMPLES:** th.pixivreplacer reply, th.pr delete, th.pr off")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task PrefixAsync([Remainder] string? twrOption = null)
        {
            if (Context.User.IsBot) return;

            var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
            if (serverSettings == null) return;
            if (twrOption == null)
            {
                var noOptionEmbed = await EmbedHandler.CreateUserErrorEmbed("PixivReplacer", "No option entered! Please use the command with one of the following options: reply, delete, off.\n" +
                    $"**EXAMPLE:** {serverSettings.Prefix}.pixivreplacer off");
                await ReplyAsync(embed: noOptionEmbed);
                return;
            }

            twrOption = twrOption.ToLower();

            if (twrOption == "reply")
            {
                var embed = await EmbedHandler.CreateBasicEmbed("PixivReplacer", "The bot will now reply to pixiv links with a fixed link.\n" +
                    "The original poster can react with the added reaction to delete the repost.");
                serverSettings.ReplacePixivLinks = "reply";
                await ReplyAsync(embed: embed);
            }
            else if (twrOption == "delete")
            {
                var embed = await EmbedHandler.CreateBasicEmbed("PixivReplacer", "The bot will now delete the original message, and post a fixed pixiv link.\n" +
                    "If the message contains more than just a pixiv link, it will not delete it, and simply reply with a fixed link.\n" +
                    "The original poster can react with the added reaction to delete the repost.");
                serverSettings.ReplacePixivLinks = "delete";
                await ReplyAsync(embed: embed);
            }
            else if (twrOption == "off")
            {
                var embed = await EmbedHandler.CreateBasicEmbed("PixivReplacer", "The bot will now ignore pixiv links and not do anything about them.");
                serverSettings.ReplacePixivLinks = "off";
                await ReplyAsync(embed: embed);
            }
            else
            {
                var invalidOptionEmbed = await EmbedHandler.CreateUserErrorEmbed("PixivReplacer", $"\"{twrOption}\" is not a valid option! Please use the command with one of the following options: reply, delete, off.\n" +
                    $"**EXAMPLE:** {serverSettings.Prefix}pixivreplacer off");
                await ReplyAsync(embed: invalidOptionEmbed);
                return;
            }

            var update = Builders<ServerSpecifics>.Update.Set(x => x.ReplacePixivLinks, serverSettings.ReplacePixivLinks);
            await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
        }
    }
}
