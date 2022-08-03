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
                    var existingRec = db.LoadRecordById<Blacklist>("BlacklistedChannels", Context.Guild.Id);

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
                        if (!existingRec.BLChannelId.Any())
                        {
                            var noBLShowEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist can't be shown", "This server has no currently blacklisted channels!");
                            await ReplyAsync(embed: noBLShowEmbed);
                        }
                        else if (flag.ToLower() == "show")
                        {
                            var BLShowEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"Current Blacklist for {Context.Guild.Name}");

                            foreach (var channel in existingRec.BLChannelId)
                            {
                                var channelName = Context.Guild.GetChannel(channel).Name;
                                BLShowEmbed = BLShowEmbed.AddField($"#{channelName}", channel, true);
                            }

                            await ReplyAsync(embed: BLShowEmbed.Build());
                        }
                        else if (flag.ToLower() == "clear")
                        {
                            existingRec.BLChannelId.Clear();
                            db.UpsertRecord("BlacklistedChannels", Context.Guild.Id, existingRec);

                            var BLClearEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist cleared!", $"Cleared blacklist for {Context.Guild.Name}");
                            await ReplyAsync(embed: BLClearEmbed);
                        }
                    }
                }
                else
                {
                    var existingRec = db.LoadRecordById<Blacklist>("BlacklistedGameChannels", Context.Guild.Id);

                    if (existingRec == null)
                    {
                        if (flag.ToLower() == "show")
                        {
                            var noBLShowEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist for games can't be shown", "This server has no currently blacklisted channels for games!");
                            await ReplyAsync(embed: noBLShowEmbed);
                        }
                        else if (flag.ToLower() == "clear")
                        {
                            var noBLClearEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist for games can't be cleared", "This server has no currently blacklisted channels for games!");
                            await ReplyAsync(embed: noBLClearEmbed);
                        }
                    }
                    else
                    {
                        if (!existingRec.BLChannelId.Any())
                        {
                            var noBLShowEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist for games can't be shown", "This server has no currently blacklisted channels for games!");
                            await ReplyAsync(embed: noBLShowEmbed);
                        }
                        else if (flag.ToLower() == "show")
                        {
                            var BLShowEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"Current game command Blacklist for {Context.Guild.Name}");

                            foreach (var channel in existingRec.BLChannelId)
                            {
                                var channelName = Context.Guild.GetChannel(channel).Name;
                                BLShowEmbed = BLShowEmbed.AddField($"#{channelName}", channel, true);
                            }

                            await ReplyAsync(embed: BLShowEmbed.Build());
                        }
                        else if (flag.ToLower() == "clear")
                        {
                            existingRec.BLChannelId.Clear();
                            db.UpsertRecord("BlacklistedGameChannels", Context.Guild.Id, existingRec);

                            var BLClearEmbed = await EmbedHandler.CreateBasicEmbed("Blacklist for games cleared!", $"Cleared game command blacklist for {Context.Guild.Name}");
                            await ReplyAsync(embed: BLClearEmbed);
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
