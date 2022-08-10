using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
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

                var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);

                if (serverSettings.BLChannelId != null)
                {
                    if (serverSettings.BLChannelId.Contains(channelId))
                    {
                        var alreadyWLEmbed = await EmbedHandler.CreateUserErrorEmbed("Whitelist", "This channel is already blacklisted, you can't whitelist a currently blacklisted channel!");
                        await ReplyAsync(embed: alreadyWLEmbed);
                        return;
                    }
                }


                if (serverSettings.WLChannelId == null)
                {
                    var channelIdList = new List<ulong> { channelId };
                    serverSettings.WLChannelId = channelIdList;
                    var update = Builders<ServerSpecifics>.Update.Set(x => x.WLChannelId, serverSettings.WLChannelId);
                    await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel whitelisted", $"Whitelisted channel #{Context.Client.GetChannel(channelId)}.");
                    await ReplyAsync(embed: embed);
                }
                else if (serverSettings.WLChannelId.Contains(channelId))
                {
                    serverSettings.WLChannelId.Remove(channelId);
                    var update = Builders<ServerSpecifics>.Update.Set(x => x.WLChannelId, serverSettings.WLChannelId);
                    await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel removed from whitelist.", $"Removed channel #{Context.Client.GetChannel(channelId)} from the whitelist.");
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    serverSettings.WLChannelId.Add(channelId);
                    var update = Builders<ServerSpecifics>.Update.Set(x => x.WLChannelId, serverSettings.WLChannelId);
                    await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel whitelisted", $"Whitelisted channel #{Context.Client.GetChannel(channelId)}.");
                    await ReplyAsync(embed: embed);
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
