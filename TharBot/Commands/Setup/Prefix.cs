using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Prefix : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;

        public Prefix(IConfiguration configuration)
        {
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", configuration);
        }

        [Command("Prefix")]
        [Summary("Displays the prefix, or changes it to the prefix given.\n" +
            "**USAGE:** th.prefix, th.prefix [NEW_PREFIX]\n" +
            "**EXAMPLE:** th.prefix !")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task PrefixAsync([Remainder] string? prefix = null)
        {
            var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
            string? currentPrefix;

            if (serverSettings.Prefix != null) currentPrefix = serverSettings.Prefix;
            else currentPrefix = _configuration["Prefix"];

            if (prefix == null)
            {
                var embed = await EmbedHandler.CreateBasicEmbed("Prefix", $"Current prefix for this server is \"{currentPrefix}\"");
                await ReplyAsync(embed: embed);
            }
            else
            {
                serverSettings.Prefix = prefix;
                var update = Builders<ServerSpecifics>.Update.Set(x => x.Prefix, serverSettings.Prefix);
                await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
               
                var embed = await EmbedHandler.CreateBasicEmbed("Prefix", $"Changed prefix for this server to \"{serverSettings.Prefix}\"");
                await ReplyAsync(embed: embed);
            }
        }
    }
}
