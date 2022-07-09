using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class WhitelistCmd : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public WhitelistCmd(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("Whitelist")]
        [Alias("WL")]
        [Summary("Shows or clears the current whitelist for this server.\n" +
            "**USAGE:** th.whitelist [FLAG]\n" +
            "**EXAMPLE:** th.whitelist show, th.wl clear")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task WhitelistAsync(string flag)
        {
            if (flag.ToLower() != "show" && flag.ToLower() != "clear")
            {
                var wrongFlagEmbed = await EmbedHandler.CreateUserErrorEmbed("Whitelist", "Please specify either \"clear\" or \"show\" after the command.");
                await ReplyAsync(embed: wrongFlagEmbed);
            }

            try
            {
                var existingRec = db.LoadRecordById<Whitelist>("WhitelistedChannels", Context.Guild.Id);

                if (existingRec == null)
                {
                    if (flag.ToLower() == "show")
                    {
                        var noWLShowEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist can't be shown", "This server has no currently whitelisted channels!");
                        await ReplyAsync(embed: noWLShowEmbed);
                    }
                    else if (flag.ToLower() == "clear")
                    {
                        var noWLClearEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist can't be cleared", "This server has no currently whitelisted channels!");
                        await ReplyAsync(embed: noWLClearEmbed);
                    }
                }
                else
                {
                    if (!existingRec.WLChannelId.Any())
                    {
                        var noWLShowEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist can't be shown", "This server has no currently whitelisted channels!");
                        await ReplyAsync(embed: noWLShowEmbed);
                    }
                    else if (flag.ToLower() == "show")
                    {
                        var WLShowEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"Current whitelist for {Context.Guild.Name}");

                        foreach (var channel in existingRec.WLChannelId)
                        {
                            var channelName = Context.Guild.GetChannel(channel).Name;
                            WLShowEmbed = WLShowEmbed.AddField($"#{channelName}", channel, true);
                        }

                        await ReplyAsync(embed: WLShowEmbed.Build());
                    }
                    else if (flag.ToLower() == "clear")
                    {
                        existingRec.WLChannelId.Clear();
                        db.UpsertRecord("WhitelistedChannels", Context.Guild.Id, existingRec);

                        var WLClearEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist cleared!", $"Cleared whitelist for {Context.Guild.Name}");
                        await ReplyAsync(embed: WLClearEmbed);
                    }
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Whitelist", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Whitelist", null, ex);
            }
        }
    }
}
