using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class WhitelistChannel : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public WhitelistChannel(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("WLChannel")]
        [Alias("WhitelistChannel", "wlc")]
        [Summary("Specifies a channel to be whitelisted or removed from the whitelist. If no channel specified, will whitelist the channel command is used in." +
            "If the server has any whitelisted channels, those are the *only* channels the bot will respond to messages in.\n" +
            "Results from pulsecheck will ignore the whitelist.\n" +
            "**USAGE:** th.wlc, th.wlchannel [CHANNEL_ID]\n" +
            "**EXAMPLE:** th.wlchannel, th.wlc 828868196324212747")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task WhitelistChannelAsync(ulong channelId = 0)
        {
            try
            {
                if (channelId == 0) channelId = Context.Channel.Id;

                var existingBL = db.LoadRecordById<Blacklist>("BlacklistedChannels", Context.Guild.Id);

                if (existingBL != null)
                {
                    if (existingBL.BLChannelId.Contains(channelId))
                    {
                        var alreadyWLEmbed = await EmbedHandler.CreateUserErrorEmbed("Whitelist", "This channel is already blacklisted, you can't whitelist a previously blacklisted channel!");
                        await ReplyAsync(embed: alreadyWLEmbed);
                        return;
                    }
                }

                var existingRec = db.LoadRecordById<Whitelist>("WhitelistedChannels", Context.Guild.Id);

                if (existingRec == null)
                {
                    var channelIdList = new List<ulong> { channelId };
                    var newWhitelist = new Whitelist
                    {
                        ServerId = Context.Guild.Id,
                        WLChannelId = channelIdList
                    };
                    db.InsertRecord("WhitelistedChannels", newWhitelist);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel whitelisted", $"Whitelisted channel #{Context.Client.GetChannel(channelId)}");
                    await ReplyAsync(embed: embed);
                }
                else if (existingRec.WLChannelId.Contains(channelId))
                {
                    existingRec.WLChannelId.Remove(channelId);
                    db.UpsertRecord("WhitelistedChannels", Context.Guild.Id, existingRec);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel removed from whitelist", $"Removed channel #{Context.Client.GetChannel(channelId)} from the whitelist");
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    existingRec.WLChannelId.Add(channelId);
                    db.UpsertRecord("WhitelistedChannels", Context.Guild.Id, existingRec);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel whitelisted", $"Whitelisted channel #{Context.Client.GetChannel(channelId)}");
                    await ReplyAsync(embed: embed);
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("WhitelistChannel", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: WhitelistChannel", null, ex);
            }
        }
    }
}
