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
        public async Task BlacklistGameChannelAsync(ulong channelId = 0)
        {
            try
            {
                if (channelId == 0) channelId = Context.Channel.Id;

                var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
                var existingWL = serverSettings.GameWLChannelId;

                if (existingWL != null)
                {
                    if (existingWL.Contains(channelId))
                    {
                        var alreadyWLEmbed = await EmbedHandler.CreateUserErrorEmbed("Blacklist", "This channel is already whitelisted for games, you can't blacklist a currently whitelisted channel!");
                        await ReplyAsync(embed: alreadyWLEmbed);
                        return;
                    }
                }


                if (serverSettings.GameBLChannelId == null)
                {
                    var channelIdList = new List<ulong> { channelId };
                    serverSettings.GameBLChannelId = channelIdList;
                    await db.UpsertRecordAsync("ServerSpecifics", Context.Guild.Id, serverSettings);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel blacklisted for games", $"Blacklisted channel #{Context.Client.GetChannel(channelId)} for games.");
                    await ReplyAsync(embed: embed);
                }
                else if (serverSettings.GameBLChannelId.Contains(channelId))
                {
                    serverSettings.GameBLChannelId.Remove(channelId);
                    await db.UpsertRecordAsync("ServerSpecifics", Context.Guild.Id, serverSettings);
                    var embed = await EmbedHandler.CreateBasicEmbed("Channel removed from blacklist for games.", $"Removed channel #{Context.Client.GetChannel(channelId)} from the blacklist for games.");
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    serverSettings.GameBLChannelId.Add(channelId);
                    await db.UpsertRecordAsync("ServerSpecifics", Context.Guild.Id, serverSettings);
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
