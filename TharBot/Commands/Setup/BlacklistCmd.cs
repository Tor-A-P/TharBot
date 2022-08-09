using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class BlacklistCmd : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;
        private readonly IConfiguration _config;

        public BlacklistCmd(IConfiguration config)
        {
            _config = config;
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("Blacklist")]
        [Alias("BL")]
        [Summary("Shows or clears the current blacklist for this server.\n" +
            "**USAGE:** th.blacklist [FLAG] [OPTIONAL_TYPE]\n" +
            "**EXAMPLE:** th.blacklist show, th.bl clear, th.bl show game")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task BlacklistAsync(string flag = "show", string type = "")
        {
            if (flag.ToLower() != "show" && flag.ToLower() != "clear")
            {
                var wrongFlagEmbed = await EmbedHandler.CreateUserErrorEmbed("Blacklist", "Please specify either \"clear\" or \"show\" after the command, or leave it blank to default to showing the blacklist.");
                await ReplyAsync(embed: wrongFlagEmbed);
                return;
            }
            if (type.ToLower() != "game" && type.ToLower() != "")
            {
                var wrongTypeEmbed = await EmbedHandler.CreateUserErrorEmbed("Blacklist", "Please use \"game\" as the optional type.");
                await ReplyAsync(embed: wrongTypeEmbed);
                return;
            }

            try
            {
                if (type == "")
                {
                    var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
                    var existingRec = serverSettings.BLChannelId;

                    if (existingRec == null)
                    {
                        if (flag.ToLower() == "show")
                        {
                            var noBLShowEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist can't be shown", "This server has no currently blacklisted channels!");
                            await ReplyAsync(embed: noBLShowEmbed);
                        }
                        else if (flag.ToLower() == "clear")
                        {
                            var noBLClearEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist can't be cleared", "This server has no currently blacklisted channels!");
                            await ReplyAsync(embed: noBLClearEmbed);
                        }
                    }
                    else
                    {
                        if (!existingRec.Any())
                        {
                            var noBLShowEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist can't be shown", "This server has no currently blacklisted channels!");
                            await ReplyAsync(embed: noBLShowEmbed);
                        }
                        else if (flag.ToLower() == "show")
                        {
                            var BLShowEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"Current Blacklist for {Context.Guild.Name}");

                            foreach (var channel in existingRec)
                            {
                                var channelName = Context.Guild.GetChannel(channel).Name;
                                BLShowEmbed = BLShowEmbed.AddField($"#{channelName}", channel, true);
                            }

                            await ReplyAsync(embed: BLShowEmbed.Build());
                        }
                        else if (flag.ToLower() == "clear")
                        {
                            existingRec.Clear();
                            await db.UpsertRecordAsync("ServerSpecifics", Context.Guild.Id, serverSettings);

                            var BLClearEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist cleared!", $"Cleared blacklist for {Context.Guild.Name}");
                            await ReplyAsync(embed: BLClearEmbed);
                        }
                    }
                }
                else
                {
                    var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
                    var existingRec = serverSettings.GameBLChannelId;

                    if (existingRec == null)
                    {
                        if (flag.ToLower() == "show")
                        {
                            var noGameBLShowEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist for games can't be shown", "This server has no currently blacklisted channels for games!");
                            await ReplyAsync(embed: noGameBLShowEmbed);
                        }
                        else if (flag.ToLower() == "clear")
                        {
                            var noGameBLClearEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist for games can't be cleared", "This server has no currently blacklisted channels for games!");
                            await ReplyAsync(embed: noGameBLClearEmbed);
                        }
                    }
                    else
                    {
                        if (!existingRec.Any())
                        {
                            var noGameBLShowEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist for games can't be shown", "This server has no currently blacklisted channels for games!");
                            await ReplyAsync(embed: noGameBLShowEmbed);
                        }
                        else if (flag.ToLower() == "show")
                        {
                            var GameBLShowEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"Current game command Blacklist for {Context.Guild.Name}");

                            foreach (var channel in existingRec)
                            {
                                var channelName = Context.Guild.GetChannel(channel).Name;
                                GameBLShowEmbed = GameBLShowEmbed.AddField($"#{channelName}", channel, true);
                            }

                            await ReplyAsync(embed: GameBLShowEmbed.Build());
                        }
                        else if (flag.ToLower() == "clear")
                        {
                            existingRec.Clear();
                            await db.UpsertRecordAsync("ServerSpecifics", Context.Guild.Id, serverSettings);

                            var GameBLClearEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist for games cleared!", $"Cleared game command blacklist for {Context.Guild.Name}");
                            await ReplyAsync(embed: GameBLClearEmbed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Blacklist", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Blacklist", null, ex);
            }
        }
    }
}
