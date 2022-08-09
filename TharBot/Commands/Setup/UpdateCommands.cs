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
            var existingBL = await db.LoadRecordByIdAsync<Blacklist>("BlacklistedChannels", Context.Guild.Id);
            if (existingBL == null)
            {
                existingBL = new Blacklist
                {
                    ServerId = Context.Guild.Id,
                    BLChannelId = new List<ulong>()
                };
            }

            var existingWL = await db.LoadRecordByIdAsync<Whitelist>("WhitelistedChannels", Context.Guild.Id);
            if (existingWL == null)
            {
                existingWL = new Whitelist
                {
                    ServerId = Context.Guild.Id,
                    WLChannelId = new List<ulong>()
                };
            }

            var existingGameBL = await db.LoadRecordByIdAsync<Blacklist>("BlacklistedGameChannels", Context.Guild.Id);
            if (existingGameBL == null)
            {
                existingGameBL = new Blacklist
                {
                    ServerId = Context.Guild.Id,
                    BLChannelId = new List<ulong>()
                };
            }

            var existingGameWL = await db.LoadRecordByIdAsync<Whitelist>("WhitelistedGameChannels", Context.Guild.Id);
            if (existingGameWL == null)
            {
                existingGameWL = new Whitelist
                {
                    ServerId = Context.Guild.Id,
                    WLChannelId = new List<ulong>()
                };
            }

            var existingMeme = await db.LoadRecordByIdAsync<MemeCommands>("Memes", Context.Guild.Id);
            if (existingMeme == null)
            {
                existingMeme = new MemeCommands
                {
                    ServerId = Context.Guild.Id,
                    Memes = new Dictionary<string, string>()
                };
            }

            var existingPrefix = await db.LoadRecordByIdAsync<Prefixes>("Prefixes", Context.Guild.Id);
            if (existingPrefix == null)
            {
                existingPrefix = new Prefixes
                {
                    ServerId = Context.Guild.Id,
                    Prefix = "th."
                };
            }
            var existingPCRC = await db.LoadRecordByIdAsync<PulseCheckResultsChannel>("PulsecheckResultsChannel", Context.Guild.Id);
            if (existingPCRC == null)
            {
                existingPCRC = new PulseCheckResultsChannel
                {
                    ServerId = Context.Guild.Id,
                    ResultsChannel = Context.Channel.Id
                };
            }

            var existingDailyPC = await db.LoadRecordByIdAsync<DailyPulseCheck>("DailyPulseCheck", Context.Guild.Id);

            var existingLvlUpMsg = (await db.LoadRecordByIdAsync<GameServerProfile>("GameProfiles", Context.Guild.Id)).ShowLevelUpMessage;

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
            await db.UpsertRecordAsync("ServerSpecifics", Context.Guild.Id, serverSettings);
            var embed = await EmbedHandler.CreateBasicEmbed("Server settings updated to new format!", "Reminder that this wipes the poll and reminder lists, re-add anything important manually.");
            await ReplyAsync(embed: embed);
        }

        [Command("Updateprofiles")]
        [Summary("Converts profiles to the new format")]
        [RequireOwner]
        public async Task UpdateProfilesAsync()
        {
            var oldServerProfile = await db.LoadRecordByIdAsync<GameServerProfile>("GameProfiles", Context.Guild.Id);
            var numProfiles = 0;
            foreach (var oldUserProfile in oldServerProfile.Users)
            {
                var newUserProfile = await db.LoadRecordByIdAsync<GameUser>("UserProfiles", oldUserProfile.UserId);
                if (newUserProfile == null)
                {
                    newUserProfile = new GameUser
                    {
                        UserId = oldUserProfile.UserId,
                        Servers = new List<GameServerStats>()
                    };
                }

                var newServerStats = newUserProfile.Servers.Where(x => x.ServerId == oldServerProfile.ServerId).FirstOrDefault();
                if (newServerStats == null)
                {
                    newServerStats = new GameServerStats
                    {
                        ServerId = oldServerProfile.ServerId,
                        NextRewards = DateTime.UtcNow + TimeSpan.FromMinutes(1),
                        TharCoins = oldUserProfile.TharCoins,
                        Exp = oldUserProfile.Exp,
                        Level = oldUserProfile.Level,
                        Attributes = oldUserProfile.Attributes,
                        AttributePoints = oldUserProfile.AttributePoints,
                        NumMessages = oldUserProfile.NumMessages,
                        NumFightsWon = oldUserProfile.NumFightsWon,
                        LastRespec = oldUserProfile.LastRespec,
                        FightInProgress = false,
                        GambaInProgress = false,
                        FightsThisHour = oldUserProfile.FightsThisHour,
                        FightPeriodStart = oldUserProfile.FightPeriodStart,
                        CurrentHP = oldUserProfile.CurrentHP,
                        CurrentMP = oldUserProfile.CurrentMP,
                        Debuffs = new GameDebuffs
                        {
                            StunDuration = 0,
                            HoTDuration = 0,
                            HoTStrength = 0,
                            DoTDuration = 0,
                            DoTStrength = 0
                        }
                    };
                    newUserProfile.Servers.Add(newServerStats);
                }

                await db.UpsertRecordAsync("UserProfiles", oldUserProfile.UserId, newUserProfile);
                numProfiles++;
            }

            var embed = await EmbedHandler.CreateBasicEmbed("Updated profiles!", $"Updated {numProfiles} profiles to the new format! Hopefully nothing broke too bad...");
            await ReplyAsync(embed: embed);
        }
    }
}
