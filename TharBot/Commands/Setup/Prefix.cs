using Discord.Commands;
using Microsoft.Extensions.Configuration;
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
            var existingPrefix = db.LoadRecordById<Prefixes>("Prefixes", Context.Guild.Id);

            if (existingPrefix == null)
            {
                if (prefix == null)
                {
                    var embed = await EmbedHandler.CreateBasicEmbed("Prefix", $"Current prefix for this server is \"{_configuration["Prefix"]}\"");
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    var newPrefix = new Prefixes
                    {
                        ServerId = Context.Guild.Id,
                        Prefix = prefix,
                    };
                    db.InsertRecord("Prefixes", newPrefix);
                    var embed = await EmbedHandler.CreateBasicEmbed("Prefix", $"Changed prefix for this server to \"{newPrefix.Prefix}\"");
                    await ReplyAsync(embed: embed);
                }
            }
            else
            {
                if (prefix == null)
                {
                    var embed = await EmbedHandler.CreateBasicEmbed("Prefix", $"Current prefix for this server is \"{existingPrefix.Prefix}\"");
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    existingPrefix.Prefix = prefix;
                    db.UpsertRecord("Prefixes", Context.Guild.Id, existingPrefix);
                    var embed = await EmbedHandler.CreateBasicEmbed("Prefix", $"Changed prefix for this server to \"{existingPrefix.Prefix}\"");
                    await ReplyAsync(embed: embed);
                }
            }
        }
    }
}
