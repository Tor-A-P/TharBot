﻿using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class WhitelistGame : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public WhitelistGame(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("WLGame")]
        [Alias("WhitelistGame", "wlg")]
        [Summary("Specifies a channel to be whitelisted or removed from the whitelist for game commands specifically. If no channel specified, will whitelist the channel command is used in." +
            "If the server has any whitelisted game channels, those are the *only* channels the bot will respond to game commands in.\n" +
            "**USAGE:** th.wlg, th.wlgame [CHANNEL_ID]\n" +
            "**EXAMPLE:** th.wlgame, th.wlg 828868196324212747")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task WhitelistChannelAsync(ulong channelId = 0)
        {
            try
            {
                if (channelId == 0) channelId = Context.Channel.Id;

                var existingBL = db.LoadRecordById<Blacklist>("BlacklistedGameChannels", Context.Guild.Id);

                if (existingBL != null)
                {
                    if (existingBL.BLChannelId.Contains(channelId))
                    {
                        var alreadyWLEmbed = await EmbedHandler.CreateUserErrorEmbed("Whitelist", "This channel is already blacklisted for games, you can't whitelist a previously blacklisted channel!");
                        await ReplyAsync(embed: alreadyWLEmbed);
                        return;
                    }
                }

                var existingRec = db.LoadRecordById<Whitelist>("WhitelistedGameChannels", Context.Guild.Id);

                if (existingRec == null)
                {
                    var channelIdList = new List<ulong> { channelId };
                    var newWhitelist = new Whitelist
                    {
                        ServerId = Context.Guild.Id,
                        WLChannelId = channelIdList
                    };
                    db.InsertRecord("WhitelistedGameChannels", newWhitelist);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel whitelisted for games", $"Whitelisted channel #{Context.Client.GetChannel(channelId)} for games.");
                    await ReplyAsync(embed: embed);
                }
                else if (existingRec.WLChannelId.Contains(channelId))
                {
                    existingRec.WLChannelId.Remove(channelId);
                    db.UpsertRecord("WhitelistedGameChannels", Context.Guild.Id, existingRec);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel removed from whitelist for games", $"Removed channel #{Context.Client.GetChannel(channelId)} from the whitelist for games.");
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    existingRec.WLChannelId.Add(channelId);
                    db.UpsertRecord("WhitelistedGameChannels", Context.Guild.Id, existingRec);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel whitelisted for games", $"Whitelisted channel #{Context.Client.GetChannel(channelId)} for games.");
                    await ReplyAsync(embed: embed);
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("WhitelistGame", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: WhitelistGame", null, ex);
            }
        }
    }
}