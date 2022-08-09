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
            var serverSettings = db.LoadRecordById<ServerSpecifics>("ServerSpecifics", forGuildId.Guild.Id);

            if (serverSettings == null)
            {
                serverSettings = new ServerSpecifics
                {
                    ServerId = forGuildId.Guild.Id,
                    BLChannelId = new List<ulong>(),
                    WLChannelId = new List<ulong>(),
                    GameBLChannelId = new List<ulong>(),
                    GameWLChannelId = new List<ulong>(),
                    Memes = new Dictionary<string, string>(),
                    Polls = new List<Poll>(),
                    Prefix = "th.",
                    PCResultsChannel = null,
                    Reminders = new List<Reminders>(),
                    ShowLevelUpMessage = true
                };
                db.InsertRecord("ServerSpecifics", serverSettings);
            }

            await Task.Delay(1000);
            var existingUserProfile = db.LoadRecordById<GameUser>("UserProfiles", message.Author.Id);
            var showLevelUpMessage = serverSettings.ShowLevelUpMessage;
            

            if (serverSettings.WLChannelId != null)
            {
                if (serverSettings.WLChannelId.Any())
                {
                    if (!serverSettings.WLChannelId.Contains(socketMessage.Channel.Id)) showLevelUpMessage = false;
                }
            }

            if (serverSettings.BLChannelId != null)
            {
                if (serverSettings.BLChannelId.Any())
                {
                    if (serverSettings.BLChannelId.Contains(socketMessage.Channel.Id)) showLevelUpMessage = false;
                }
            }

            var random = new Random();
            if (existingUserProfile == null)
            {
                var newProfile = new GameUser
                {
                    UserId = message.Author.Id,
                    Servers = new List<GameServerStats>(),
                    LastSeenUsername = message.Author.Username
                };
                var newServerStats = new GameServerStats
                {
                    ServerId = forGuildId.Guild.Id,
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
                    AttributePoints = GameServerStats.StartingAttributePoints,
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
                newServerStats.CurrentHP = newServerStats.BaseHP;
                newServerStats.CurrentMP = newServerStats.BaseMP;
                newProfile.Servers.Add(newServerStats);
                db.InsertRecord("UserProfiles", newProfile);
            }
            else
            {
                var existingServerStats = existingUserProfile.Servers.Where(x => x.ServerId == forGuildId.Guild.Id).FirstOrDefault();
                if (existingServerStats == null)
                {
                    var newUserProfile = new GameServerStats
                    {
                        ServerId = forGuildId.Guild.Id,
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
                        AttributePoints = GameServerStats.StartingAttributePoints,
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
                    existingUserProfile.Servers.Add(newUserProfile);
                }
                else
                {
                    existingServerStats.NumMessages++;
                    if (existingServerStats.Debuffs == null)
                    {
                        existingServerStats.Debuffs = new GameDebuffs
                        {
                            StunDuration = 0,
                            HoTDuration = 0,
                            HoTStrength = 0,
                            DoTDuration = 0,
                            DoTStrength = 0
                        };
                    }
                    if (existingServerStats.NextRewards < DateTime.UtcNow)
                    {
                        existingServerStats.NextRewards = DateTime.UtcNow + TimeSpan.FromMinutes(1);
                        existingServerStats.TharCoins += 10;
                        existingServerStats.Exp += random.Next(8, 13);
                    }
                    if (existingServerStats.Exp >= existingServerStats.ExpToLevel)
                    {
                        string? prefix;
                        if (serverSettings.Prefix != null) prefix = serverSettings.Prefix;
                        else prefix = _configuration["Prefix"];

                        existingServerStats.Exp -= existingServerStats.ExpToLevel;
                        existingServerStats.Level++;
                        existingServerStats.CurrentHP = existingServerStats.BaseHP;
                        existingServerStats.CurrentMP = existingServerStats.BaseMP;
                        existingServerStats.AttributePoints += GameServerStats.AttributePointsPerLevel;
                        if (showLevelUpMessage)
                        {
                            var levelUpEmbed = await EmbedHandler.CreateBasicEmbedBuilder("Level up!");
                            levelUpEmbed = levelUpEmbed.WithDescription($"Congratulations {socketMessage.Author.Mention}, you've reached level {existingServerStats.Level}!\n" +
                                                                        $"Your health and mana has been refilled.\n" +
                                                                        $"You have gained {GameServerStats.AttributePointsPerLevel} attribute points, use the {prefix}attributes command to spend them!")
                                                       .WithThumbnailUrl(message.Author.GetAvatarUrl(ImageFormat.Auto, 2048) ?? message.Author.GetDefaultAvatarUrl());
                            await socketMessage.Channel.SendMessageAsync(embed: levelUpEmbed.Build());
                        }
                    }
                }
                existingUserProfile.LastSeenUsername = message.Author.Username;
                db.UpsertRecord("UserProfiles", message.Author.Id, existingUserProfile);
            }
        }
    }
}
