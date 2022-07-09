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
            "**USAGE:** th.blacklist [FLAG]\n" +
            "**EXAMPLE:** th.blacklist show, th.bl clear")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task BlacklistAsync(string flag)
        {
            if (flag.ToLower() != "show" && flag.ToLower() != "clear")
            {
                var wrongFlagEmbed = await EmbedHandler.CreateUserErrorEmbed("Blacklist", "Please specify either \"clear\" or \"show\" after the command.");
                await ReplyAsync(embed: wrongFlagEmbed);
            }

            try
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
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Blacklist", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Blacklist", null, ex);
            }
        }
    }
}
