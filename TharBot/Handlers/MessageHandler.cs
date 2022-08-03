using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TharBot.DBModels;

namespace TharBot.Handlers
{
    public class MessageHandler : DiscordClientService
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;

        public MessageHandler(DiscordSocketClient client,IConfiguration configuration, ILogger<DiscordClientService> logger)
            : base(client, logger)
        {
            _client = client;
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", _configuration);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.MessageReceived += EXPCoinOnMessage;
        }
        private async Task EXPCoinOnMessage(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message) return;
            var existingBan = db.LoadRecordById<BannedUser>("UserBanlist", socketMessage.Author.Id);
            if (existingBan != null) return;
            var forGuildId = socketMessage.Channel as SocketGuildChannel;

            await Task.Delay(1000);
            var existingServerProfile = db.LoadRecordById<GameServerProfile>("GameProfiles", forGuildId.Guild.Id);
            var showLevelUpMessage = true;
            if (existingServerProfile != null)
            {
                showLevelUpMessage = existingServerProfile.ShowLevelUpMessage;
            }

            var existingWLRec = db.LoadRecordById<Whitelist>("WhitelistedChannels", forGuildId.Guild.Id);
            if (existingWLRec != null)
            {
                if (existingWLRec.WLChannelId.Any())
                {
                    if (!existingWLRec.WLChannelId.Contains(socketMessage.Channel.Id)) showLevelUpMessage = false;
                }
            }

            var existingBLRec = db.LoadRecordById<Blacklist>("BlacklistedChannels", forGuildId.Guild.Id);
            if (existingBLRec != null)
            {
                if (existingBLRec.BLChannelId.Any())
                {
                    if (existingBLRec.BLChannelId.Contains(socketMessage.Channel.Id)) showLevelUpMessage = false;
                }
            }

            var random = new Random();
            if (existingServerProfile == null)
            {
                var newProfile = new GameServerProfile
                {
                    ServerId = forGuildId.Guild.Id,
                    Users = new List<GameUserProfile>(),
                    ShowLevelUpMessage = true
                };
                var newUserProfile = new GameUserProfile
                {
                    UserId = message.Author.Id,
                    NextRewards = DateTime.UtcNow + TimeSpan.FromMinutes(1),
                    TharCoins = 10,
                    Exp = random.Next(8, 13),
                    Level = 1,
                    Attributes = new GameStats
                    {
                        Strength = 0,
                        Dexterity = 0,
                        Intelligence = 0,
                        Constitution = 0,
                        Wisdom = 0,
                        Luck = 0
                    },
                    AttributePoints = GameUserProfile.StartingAttributePoints,
                    NumMessages = 1,
                    Debuffs = new GameDebuffs
                    {
                        StunDuration = 0,
                        HoTDuration = 0,
                        HoTStrength = 0,
                        DoTDuration = 0,
                        DoTStrength = 0
                    }
                };
                newUserProfile.CurrentHP = newUserProfile.BaseHP;
                newUserProfile.CurrentMP = newUserProfile.BaseMP;
                newProfile.Users.Add(newUserProfile);
                db.InsertRecord("GameProfiles", newProfile);
            }
            else
            {
                var existingUserProfile = existingServerProfile.Users.Where(x => x.UserId == message.Author.Id).FirstOrDefault();
                if (existingUserProfile == null)
                {
                    var newUserProfile = new GameUserProfile
                    {
                        UserId = message.Author.Id,
                        NextRewards = DateTime.UtcNow + TimeSpan.FromMinutes(1),
                        TharCoins = 10,
                        Exp = random.Next(8, 13),
                        Level = 1,
                        Attributes = new GameStats
                        {
                            Strength = 0,
                            Dexterity = 0,
                            Intelligence = 0,
                            Constitution = 0,
                            Wisdom = 0,
                            Luck = 0
                        },
                        AttributePoints = GameUserProfile.StartingAttributePoints,
                        NumMessages = 1,
                        Debuffs = new GameDebuffs
                        {
                            StunDuration = 0,
                            HoTDuration = 0,
                            HoTStrength = 0,
                            DoTDuration = 0,
                            DoTStrength = 0
                        }
                    };
                    newUserProfile.CurrentHP = newUserProfile.BaseHP;
                    newUserProfile.CurrentMP = newUserProfile.BaseMP;
                    existingServerProfile.Users.Add(newUserProfile);
                }
                else
                {
                    existingUserProfile.NumMessages++;
                    if (existingUserProfile.Debuffs == null)
                    {
                        existingUserProfile.Debuffs = new GameDebuffs
                        {
                            StunDuration = 0,
                            HoTDuration = 0,
                            HoTStrength = 0,
                            DoTDuration = 0,
                            DoTStrength = 0
                        };
                    }
                    if (existingUserProfile.NextRewards < DateTime.UtcNow)
                    {
                        existingUserProfile.NextRewards = DateTime.UtcNow + TimeSpan.FromMinutes(1);
                        existingUserProfile.TharCoins += 10;
                        existingUserProfile.Exp += random.Next(8, 13);
                    }
                    if (existingUserProfile.Exp >= existingUserProfile.ExpToLevel)
                    {
                        var prefix = "";
                        var existingPrefix = db.LoadRecordById<Prefixes>("Prefixes", forGuildId.Guild.Id);
                        if (existingPrefix != null)
                        {
                            prefix = existingPrefix.Prefix;
                        }
                        else
                        {
                            prefix = _configuration["Prefix"];
                        }
                        existingUserProfile.Exp -= existingUserProfile.ExpToLevel;
                        existingUserProfile.Level++;
                        existingUserProfile.CurrentHP = existingUserProfile.BaseHP;
                        existingUserProfile.CurrentMP = existingUserProfile.BaseMP;
                        existingUserProfile.AttributePoints += GameUserProfile.AttributePointsPerLevel;
                        if (showLevelUpMessage)
                        {
                            var levelUpEmbed = await EmbedHandler.CreateBasicEmbedBuilder("Level up!");
                            levelUpEmbed = levelUpEmbed.WithDescription($"Congratulations {socketMessage.Author.Mention}, you've reached level {existingUserProfile.Level}!\n" +
                                                                        $"Your health and mana has been refilled.\n" +
                                                                        $"You have gained {GameUserProfile.AttributePointsPerLevel} attribute points, use the {prefix}attributes command to spend them!")
                                                       .WithThumbnailUrl(message.Author.GetAvatarUrl(ImageFormat.Auto, 2048) ?? message.Author.GetDefaultAvatarUrl());
                            await socketMessage.Channel.SendMessageAsync(embed: levelUpEmbed.Build());
                        }
                    }
                }
                db.UpsertRecord("GameProfiles", forGuildId.Guild.Id, existingServerProfile);
            }
        }
    }
}
