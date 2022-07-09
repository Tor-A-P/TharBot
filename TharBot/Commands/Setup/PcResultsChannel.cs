using Discord.Commands;
using TharBot.Handlers;
using TharBot.DBModels;
using Microsoft.Extensions.Configuration;

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
        [Summary("Sets the channel that the pulsecheck command should report its results in.\n" +
            "**USAGE:** th.ResultsChannel [CHANNEL_ID], th.pcrc [CHANNEL_ID]\n" +
            "**EXAMPLE:** th.ResultsChannel 981100690195759134")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task PcResultsChannelAsync(ulong channelId)
        {
            var currentServer = db.LoadRecordById<PulseCheckResultsChannel>("PulsecheckResultsChannel", Context.Guild.Id);

            if (currentServer == null)
            {
                var newServer = new PulseCheckResultsChannel
                {
                    ServerId = Context.Guild.Id,
                    ResultsChannel = channelId
                };
                db.InsertRecord("PulsecheckResultsChannel", newServer);
            }
            else
            {
                currentServer.ResultsChannel = channelId;
                db.UpsertRecord("PulsecheckResultsChannel", Context.Guild.Id, currentServer);
            }

            var embed = await EmbedHandler.CreateBasicEmbed("Pulsecheck Channel set!", 
                $"The pulsecheck command will now report its results in #{Context.Client.GetChannel(channelId)}!");
            await ReplyAsync(embed: embed);
        }
    }
}
