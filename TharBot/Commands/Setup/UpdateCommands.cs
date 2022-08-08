using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands.Setup
{
    public class UpdateCommands : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public UpdateCommands(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("UpdateSettings")]
        [Summary("Converts settings from a bunch of different collections into one per server")]
        [RequireOwner]
        public async Task UpdateSettingsAsync()
        {
            var existingBL = db.LoadRecordById<Blacklist>("BlacklistedChannels", Context.Guild.Id);
            if (existingBL == null)
            {
                existingBL = new Blacklist
                {
                    ServerId = Context.Guild.Id,
                    BLChannelId = new List<ulong>()
                };
            }

            var existingWL = db.LoadRecordById<Whitelist>("WhitelistedChannels", Context.Guild.Id);
            if (existingWL == null)
            {
                existingWL = new Whitelist
                {
                    ServerId = Context.Guild.Id,
                    WLChannelId = new List<ulong>()
                };
            }

            var existingGameBL = db.LoadRecordById<Blacklist>("BlacklistedGameChannels", Context.Guild.Id);
            if (existingGameBL == null)
            {
                existingGameBL = new Blacklist
                {
                    ServerId = Context.Guild.Id,
                    BLChannelId = new List<ulong>()
                };
            }

            var existingGameWL = db.LoadRecordById<Whitelist>("WhitelistedGameChannels", Context.Guild.Id);
            if (existingGameWL == null)
            {
                existingGameWL = new Whitelist
                {
                    ServerId = Context.Guild.Id,
                    WLChannelId = new List<ulong>()
                };
            }

            var existingMeme = db.LoadRecordById<MemeCommands>("Memes", Context.Guild.Id);
            if (existingMeme == null)
            {
                existingMeme = new MemeCommands
                {
                    ServerId = Context.Guild.Id,
                    Memes = new Dictionary<string, string>()
                };
            }

            var existingPrefix = db.LoadRecordById<Prefixes>("Prefixes", Context.Guild.Id);
            if (existingPrefix == null)
            {
                existingPrefix = new Prefixes
                {
                    ServerId = Context.Guild.Id,
                    Prefix = "th."
                };
            }
            var existingPCRC = db.LoadRecordById<PulseCheckResultsChannel>("PulsecheckResultsChannel", Context.Guild.Id);
            if (existingPCRC == null)
            {
                existingPCRC = new PulseCheckResultsChannel
                {
                    ServerId = Context.Guild.Id,
                    ResultsChannel = Context.Channel.Id
                };
            }

            var existingDailyPC = db.LoadRecordById<DailyPulseCheck>("DailyPulseCheck", Context.Guild.Id);

            var existingLvlUpMsg = db.LoadRecordById<GameServerProfile>("GameProfiles", Context.Guild.Id).ShowLevelUpMessage;

            var serverSettings = new ServerSpecifics
            {
                ServerId = Context.Guild.Id,
                BLChannelId = existingBL.BLChannelId,
                WLChannelId = existingWL.WLChannelId,
                GameBLChannelId = existingGameBL.BLChannelId,
                GameWLChannelId = existingGameWL.WLChannelId,
                DailyPC = existingDailyPC,
                Memes = existingMeme.Memes,
                Polls = new List<Poll>(),
                Prefix = existingPrefix.Prefix,
                PCResultsChannel = existingPCRC.ResultsChannel,
                Reminders = new List<Reminders>(),
                ShowLevelUpMessage = existingLvlUpMsg
            };
            db.UpsertRecord("ServerSpecifics", Context.Guild.Id, serverSettings);
            var embed = await EmbedHandler.CreateBasicEmbed("Server settings updated to new format!", "Reminder that this wipes the poll and reminder lists, re-add anything important manually.");
            await ReplyAsync(embed: embed);
        }
    }
}
