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
            "**USAGE:** th.whitelist [FLAG] [OPTIONAL_TYPE]\n" +
            "**EXAMPLE:** th.whitelist show, th.wl clear, th.wl show game")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task WhitelistAsync(string flag = "show", string type = "")
        {
            if (flag.ToLower() != "show" && flag.ToLower() != "clear")
            {
                var wrongFlagEmbed = await EmbedHandler.CreateUserErrorEmbed("Whitelist", "Please specify either \"clear\" or \"show\" after the command, or leave it blank to default to showing the whitelist.");
                await ReplyAsync(embed: wrongFlagEmbed);
                return;
            }
            if (type.ToLower() != "game" && type.ToLower() != "")
            {
                var wrongTypeEmbed = await EmbedHandler.CreateUserErrorEmbed("Whitelist", "Please use \"game\" as the optional type.");
                await ReplyAsync(embed: wrongTypeEmbed);
                return;
            }

            try
            {
                if (type == "")
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
                else
                {
                    var existingRec = db.LoadRecordById<Whitelist>("WhitelistedGameChannels", Context.Guild.Id);

                    if (existingRec == null)
                    {
                        if (flag.ToLower() == "show")
                        {
                            var noWLShowEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist for games can't be shown", "This server has no currently whitelisted channels for games!");
                            await ReplyAsync(embed: noWLShowEmbed);
                        }
                        else if (flag.ToLower() == "clear")
                        {
                            var noWLClearEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist for games can't be cleared", "This server has no currently whitelisted channels for games!");
                            await ReplyAsync(embed: noWLClearEmbed);
                        }
                    }
                    else
                    {
                        if (!existingRec.WLChannelId.Any())
                        {
                            var noWLShowEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist for games can't be shown", "This server has no currently whitelisted channels for games!");
                            await ReplyAsync(embed: noWLShowEmbed);
                        }
                        else if (flag.ToLower() == "show")
                        {
                            var WLShowEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"Current game command whitelist for {Context.Guild.Name}");

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
                            db.UpsertRecord("WhitelistedGameChannels", Context.Guild.Id, existingRec);

                            var WLClearEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist for games cleared!", $"Cleared game command whitelist for {Context.Guild.Name}");
                            await ReplyAsync(embed: WLClearEmbed);
                        }
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
