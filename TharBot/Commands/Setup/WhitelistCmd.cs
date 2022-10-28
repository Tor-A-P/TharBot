using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
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
            if (Context.User.IsBot) return;

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
                    var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
                    var existingRec = serverSettings.WLChannelId;

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
                        if (!existingRec.Any())
                        {
                            var noWLShowEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist can't be shown", "This server has no currently whitelisted channels!");
                            await ReplyAsync(embed: noWLShowEmbed);
                        }
                        else if (flag.ToLower() == "show")
                        {
                            var WLShowEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"Current Whitelist for {Context.Guild.Name}");

                            foreach (var channel in existingRec)
                            {
                                var channelName = Context.Guild.GetChannel(channel).Name;
                                WLShowEmbed = WLShowEmbed.AddField($"#{channelName}", channel, true);
                            }

                            await ReplyAsync(embed: WLShowEmbed.Build());
                        }
                        else if (flag.ToLower() == "clear")
                        {
                            existingRec.Clear();
                            var update = Builders<ServerSpecifics>.Update.Set(x => x.WLChannelId, serverSettings.WLChannelId);
                            await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);

                            var WLClearEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist cleared!", $"Cleared whitelist for {Context.Guild.Name}");
                            await ReplyAsync(embed: WLClearEmbed);
                        }
                    }
                }
                else
                {
                    var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
                    var existingRec = serverSettings.GameWLChannelId;

                    if (existingRec == null)
                    {
                        if (flag.ToLower() == "show")
                        {
                            var noGameWLShowEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist for games can't be shown", "This server has no currently whitelisted channels for games!");
                            await ReplyAsync(embed: noGameWLShowEmbed);
                        }
                        else if (flag.ToLower() == "clear")
                        {
                            var noGameWLClearEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist for games can't be cleared", "This server has no currently whitelisted channels for games!");
                            await ReplyAsync(embed: noGameWLClearEmbed);
                        }
                    }
                    else
                    {
                        if (!existingRec.Any())
                        {
                            var noGameWLShowEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist for games can't be shown", "This server has no currently whitelisted channels for games!");
                            await ReplyAsync(embed: noGameWLShowEmbed);
                        }
                        else if (flag.ToLower() == "show")
                        {
                            var GameWLShowEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"Current game command whitelist for {Context.Guild.Name}");

                            foreach (var channel in existingRec)
                            {
                                var channelName = Context.Guild.GetChannel(channel).Name;
                                GameWLShowEmbed = GameWLShowEmbed.AddField($"#{channelName}", channel, true);
                            }

                            await ReplyAsync(embed: GameWLShowEmbed.Build());
                        }
                        else if (flag.ToLower() == "clear")
                        {
                            existingRec.Clear();
                            var update = Builders<ServerSpecifics>.Update.Set(x => x.GameWLChannelId, serverSettings.GameWLChannelId);
                            await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);

                            var GameWLClearEmbed = await EmbedHandler.CreateBasicEmbed("Whitelist for games cleared!", $"Cleared game command whitelist for {Context.Guild.Name}");
                            await ReplyAsync(embed: GameWLClearEmbed);
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
