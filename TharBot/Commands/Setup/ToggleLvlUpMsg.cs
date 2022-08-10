using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands.Setup
{
    public class ToggleLvlUpMsg : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public ToggleLvlUpMsg(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }
        [Command("ToggleLvlUp")]
        [Alias("tlu")]
        [Summary("Toggles whether or not the bot should show messages when someone levels up in this server.\n" +
            "**USAGE:** -ToggleLvlUp")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task ToggleLvlUpMsgAsync()
        {
            var serverSpecifics = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
            if (serverSpecifics == null)
            {
                var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find server profile", "It seems this server has no profile, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noServerProfEmbed);
                return;
            }

            serverSpecifics.ShowLevelUpMessage = !serverSpecifics.ShowLevelUpMessage;
            var update = Builders<ServerSpecifics>.Update.Set(x => x.ShowLevelUpMessage, serverSpecifics.ShowLevelUpMessage);
            await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
            string? embedMsg;
            if (serverSpecifics.ShowLevelUpMessage) embedMsg = "I will now show a message every time someone levels up in this server!";
            else embedMsg = "I will no longer show a message every time someone levels up in this server!";
            var embed = await EmbedHandler.CreateBasicEmbed("Toggled showing level up message", embedMsg);
            await ReplyAsync(embed: embed);
        }
    }
}
