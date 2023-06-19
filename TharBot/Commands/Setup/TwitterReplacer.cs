using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TharBot.DBModels;
using TharBot.Handlers;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;

namespace TharBot.Commands
{
    public class TwitterReplacer : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;

        public TwitterReplacer(IConfiguration configuration)
        {
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", configuration);
        }

        [Command("TwitterReplacer")]
        [Alias("twr")]
        [Summary("Toggles whether or not the bot should reply to twitter links with a link free of twitter trackers and using vxtwitter, or delete the message containing the link entirely\n" +
            "If a message contains more text than only the twitter link, the delete option will function like the reply option instead\n" +
            "**USAGE:** th.twitterreplacer [OPTION]\n" +
            "**EXAMPLES:** th.twitterreplacer reply, th.twr delete, th.twr off")]
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
                var noOptionEmbed = await EmbedHandler.CreateUserErrorEmbed("TwitterReplacer", "No option entered! Please use the command with one of the following options: reply, delete, off.\n" +
                    $"**EXAMPLE:** {serverSettings.Prefix}.twitterreplacer off");
                await ReplyAsync(embed: noOptionEmbed);
                return;
            }
            twrOption = twrOption.ToLower();

            if (twrOption == "reply")
            {
                var embed = await EmbedHandler.CreateBasicEmbed("TwitterReplacer", $"The bot will now reply to twitter links with a fixed link.");
                serverSettings.ReplaceTwitterLinks = "reply";
                await ReplyAsync(embed: embed);
            }
            else if (twrOption == "delete")
            {
                var embed = await EmbedHandler.CreateBasicEmbed("TwitterReplacer", "The bot will now delete the original message, and post a fixed twitter link.\n" +
                    "If the message contains more than just a twitter link, it will not delete ir, and simply reply with a fixed link.");
                serverSettings.ReplaceTwitterLinks = "delete";
                await ReplyAsync(embed: embed);
            }
            else if (twrOption == "off")
            {
                var embed = await EmbedHandler.CreateBasicEmbed("TwitterReplacer", "The bot will now ignore twitter links and not do anything about them.");
                serverSettings.ReplaceTwitterLinks = "off";
                await ReplyAsync(embed: embed);
            }
            else
            {
                var invalidOptionEmbed = await EmbedHandler.CreateUserErrorEmbed("TwitterReplacer", $"\"{twrOption}\" is not a valid option! Please use the command with one of the following options: reply, delete, off.\n" +
                    $"**EXAMPLE:** {serverSettings.Prefix}.twitterreplacer off");
                await ReplyAsync(embed: invalidOptionEmbed);
                return;
            }

            var update = Builders<ServerSpecifics>.Update.Set(x => x.ReplaceTwitterLinks, serverSettings.ReplaceTwitterLinks);
            await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
        }
    }
}
