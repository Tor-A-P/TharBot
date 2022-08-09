using Discord.Commands;
using TharBot.Handlers;
using TharBot.DBModels;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace TharBot.Commands
{
    public class PcResultsChannel : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public PcResultsChannel(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("ResultsChannel")]
        [Alias("PcResultsChannel", "pcrc")]
        [Summary("Sets the channel that the pulsecheck command should report its results in. If no channel specified, will set it to the channel the command is used in.\n" +
            "**USAGE:** th.ResultsChannel [CHANNEL_ID], th.ResultsChannel\n" +
            "**EXAMPLE:** th.ResultsChannel 981100690195759134")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task PcResultsChannelAsync(ulong channelId = 0)
        {
            var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);

            if (channelId == 0)
            {
                channelId = Context.Channel.Id;
            }

            serverSettings.PCResultsChannel = channelId;
            var update = Builders<ServerSpecifics>.Update.Set(x => x.PCResultsChannel, serverSettings.PCResultsChannel);
            await db.UpsertServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);

            var embed = await EmbedHandler.CreateBasicEmbed("Pulsecheck Channel set!", 
                $"The pulsecheck command will now report its results in #{Context.Client.GetChannel(channelId)}!");
            await ReplyAsync(embed: embed);
        }
    }
}
