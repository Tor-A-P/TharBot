using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class BlacklistChannel : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public BlacklistChannel(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("BLChannel")]
        [Alias("BlacklistChannel", "blc")]
        [Summary("Specifies a channel for the bot to ignore or stop ignoring. If no channel specified, will blacklist the channel command is used in.\n" +
            "The results from a pulsecheck will still be posted even if the results channel is blacklisted.\n" +
            "**USAGE:** th.blc, th.blchannel [CHANNEL_ID]\n" +
            "**EXAMPLE:** th.blchannel, th.blc 828868196324212747")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task BlacklistChannelAsync(ulong channelId = 0)
        {
            try
            {
                if (channelId == 0) channelId = Context.Channel.Id;

                var existingWL = db.LoadRecordById<Whitelist>("WhitelistedChannels", Context.Guild.Id);

                if (existingWL != null)
                {
                    if (existingWL.WLChannelId.Contains(channelId))
                    {
                        var alreadyWLEmbed = await EmbedHandler.CreateUserErrorEmbed("Blacklist", "This channel is already whitelisted, you can't blacklist a previously whitelisted channel!");
                        await ReplyAsync(embed: alreadyWLEmbed);
                        return;
                    }
                }

                var existingRec = db.LoadRecordById<Blacklist>("BlacklistedChannels", Context.Guild.Id);

                if (existingRec == null)
                {
                    var channelIdList = new List<ulong> { channelId };
                    var newBlacklist = new Blacklist
                    {
                        ServerId = Context.Guild.Id,
                        BLChannelId = channelIdList
                    };
                    db.InsertRecord("BlacklistedChannels", newBlacklist);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel blacklisted", $"Blacklisted channel #{Context.Client.GetChannel(channelId)}");
                    await ReplyAsync(embed: embed);
                }
                else if (existingRec.BLChannelId.Contains(channelId))
                {
                    existingRec.BLChannelId.Remove(channelId);
                    db.UpsertRecord("BlacklistedChannels", Context.Guild.Id, existingRec);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel removed from blacklist", $"Removed channel #{Context.Client.GetChannel(channelId)} from the blacklist");
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    existingRec.BLChannelId.Add(channelId);
                    db.UpsertRecord("BlacklistedChannels", Context.Guild.Id, existingRec);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel blacklisted", $"Blacklisted channel #{Context.Client.GetChannel(channelId)}");
                    await ReplyAsync(embed: embed);
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("BlacklistChannel", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: BlacklistChannel", null, ex);
            }
        }
    }
}
