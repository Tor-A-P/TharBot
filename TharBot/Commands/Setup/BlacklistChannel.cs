using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
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
            if (Context.User.IsBot) return;

            try
            {
                if (channelId == 0) channelId = Context.Channel.Id;

                var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
                var existingWL = serverSettings.WLChannelId;

                if (existingWL != null)
                {
                    if (existingWL.Contains(channelId))
                    {
                        var alreadyWLEmbed = await EmbedHandler.CreateUserErrorEmbed("Blacklist", "This channel is already whitelisted, you can't blacklist a currently whitelisted channel!");
                        await ReplyAsync(embed: alreadyWLEmbed);
                        return;
                    }
                }


                if (serverSettings.BLChannelId == null)
                {
                    var channelIdList = new List<ulong> { channelId };
                    serverSettings.BLChannelId = channelIdList;
                    var update = Builders<ServerSpecifics>.Update.Set(x => x.BLChannelId, serverSettings.BLChannelId);
                    await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel blacklisted", $"Blacklisted channel #{Context.Client.GetChannel(channelId)}.");
                    await ReplyAsync(embed: embed);
                }
                else if (serverSettings.BLChannelId.Contains(channelId))
                {
                    serverSettings.BLChannelId.Remove(channelId);
                    var update = Builders<ServerSpecifics>.Update.Set(x => x.BLChannelId, serverSettings.BLChannelId);
                    await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel removed from blacklist", $"Removed channel #{Context.Client.GetChannel(channelId)} from the blacklist.");
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    serverSettings.BLChannelId.Add(channelId);
                    var update = Builders<ServerSpecifics>.Update.Set(x => x.BLChannelId, serverSettings.BLChannelId);
                    await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel blacklisted", $"Blacklisted channel #{Context.Client.GetChannel(channelId)}.");
                    await ReplyAsync(embed: embed);
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
