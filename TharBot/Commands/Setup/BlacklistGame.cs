using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class BlacklistGame : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public BlacklistGame(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("BLGame")]
        [Alias("BlacklistGame", "blg")]
        [Summary("Specifies a channel for the bot to ignore or stop ignoring game commands in. If no channel specified, will blacklist the channel command is used in.\n" +
            "**USAGE:** th.blg, th.blgame [CHANNEL_ID]\n" +
            "**EXAMPLE:** th.blgame, th.blg 828868196324212747")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task BlacklistChannelAsync(ulong channelId = 0)
        {
            try
            {
                if (channelId == 0) channelId = Context.Channel.Id;

                var existingWL = db.LoadRecordById<Whitelist>("WhitelistedGameChannels", Context.Guild.Id);

                if (existingWL != null)
                {
                    if (existingWL.WLChannelId.Contains(channelId))
                    {
                        var alreadyWLEmbed = await EmbedHandler.CreateUserErrorEmbed("Blacklist", "This channel is already whitelisted for games, you can't blacklist a previously whitelisted channel!");
                        await ReplyAsync(embed: alreadyWLEmbed);
                        return;
                    }
                }

                var existingRec = db.LoadRecordById<Blacklist>("BlacklistedGameChannels", Context.Guild.Id);

                if (existingRec == null)
                {
                    var channelIdList = new List<ulong> { channelId };
                    var newBlacklist = new Blacklist
                    {
                        ServerId = Context.Guild.Id,
                        BLChannelId = channelIdList
                    };
                    db.InsertRecord("BlacklistedGameChannels", newBlacklist);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel blacklisted for games", $"Blacklisted channel #{Context.Client.GetChannel(channelId)} for games.");
                    await ReplyAsync(embed: embed);
                }
                else if (existingRec.BLChannelId.Contains(channelId))
                {
                    existingRec.BLChannelId.Remove(channelId);
                    db.UpsertRecord("BlacklistedGameChannels", Context.Guild.Id, existingRec);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel removed from blacklist for games.", $"Removed channel #{Context.Client.GetChannel(channelId)} from the blacklist for games.");
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    existingRec.BLChannelId.Add(channelId);
                    db.UpsertRecord("BlacklistedGameChannels", Context.Guild.Id, existingRec);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel blacklisted for games", $"Blacklisted channel #{Context.Client.GetChannel(channelId)} for games.");
                    await ReplyAsync(embed: embed);
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("BlacklistGame", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: BlacklistGame", null, ex);
            }
        }
    }
}
