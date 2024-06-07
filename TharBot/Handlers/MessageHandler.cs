using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Reflection;
using System.Text.RegularExpressions;
using TharBot.DBModels;
using static System.Net.WebRequestMethods;

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
            _client.MessageReceived += TwitterReplacer;
            _client.MessageReceived += InstagramReplacer;
            _client.MessageReceived += HeckExMeme;
        }
        private async Task EXPCoinOnMessage(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message) return;
            var existingBan = await db.LoadRecordByIdAsync<BannedUser>("UserBanlist", socketMessage.Author.Id);
            if (existingBan != null) return;
            var forGuildId = socketMessage.Channel as SocketGuildChannel;
            var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", forGuildId.Guild.Id);

            if (serverSettings == null)
            {
                serverSettings = new ServerSpecifics
                {
                    ServerId = forGuildId.Guild.Id,
                    Revision = 0,
                    BLChannelId = new List<ulong>(),
                    WLChannelId = new List<ulong>(),
                    GameBLChannelId = new List<ulong>(),
                    GameWLChannelId = new List<ulong>(),
                    AttributeDialogs = new List<GameAttributeDialog>(),
                    Memes = new Dictionary<string, string>(),
                    Polls = new List<Poll>(),
                    Prefix = "th.",
                    PCResultsChannel = null,
                    Reminders = new List<Reminders>(),
                    ShowLevelUpMessage = true
                };
                await db.InsertRecordAsync("ServerSpecifics", serverSettings);
            }

            await Task.Delay(500);
            var existingUserProfile = await db.LoadRecordByIdAsync<GameUser>("UserProfiles", message.Author.Id);
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
                    LastSeenUsername = message.Author.Username,
                    Revision = 0
                };
                var newServerStats = new GameServerStats
                {
                    ServerId = forGuildId.Guild.Id,
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
                await db.InsertRecordAsync("UserProfiles", newProfile);
            }
            else
            {
                var existingServerStats = existingUserProfile.Servers.Where(x => x.ServerId == forGuildId.Guild.Id).FirstOrDefault();
                if (existingServerStats == null)
                {
                    var newUserProfile = new GameServerStats
                    {
                        ServerId = forGuildId.Guild.Id,
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
                var update = Builders<GameUser>.Update
                    .Set(x => x.Servers, existingUserProfile.Servers)
                    .Set(x => x.LastSeenUsername, message.Author.Username);
                await db.UpdateUserAsync<GameUser>("UserProfiles", message.Author.Id, update);
            }
        }

        private async Task TwitterReplacer(SocketMessage socketMessage)
        {

            try
            {
                if (socketMessage is not SocketUserMessage message || socketMessage.Author.IsBot) return;

            var forGuildId = socketMessage.Channel as SocketGuildChannel;
            var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", forGuildId.Guild.Id);

            if (serverSettings == null) return;
            if (serverSettings.ReplaceTwitterLinks == null)
            {
                return;
            }

            if (serverSettings.ReplaceTwitterLinks == "off")
            {
                return;
            }

            Regex urlRx = new(@"((https?|ftp|file)\://|www.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\(\)\*\-\._~%]*)*", RegexOptions.IgnoreCase);
            string url = urlRx.Match(message.Content).ToString();
            string OGurl = url;

            try
            {
                if (serverSettings.ReplaceTwitterLinks == "reply" || message.Content != url)
                {
                    if (url.Contains("twitter.com") || url.Contains("twitter.com"))
                    {
                        if (url.Contains("ssstwitter") || url.Contains("cdn.discordapp.com")) return;
                        if (!url.Contains("/status/") && !url.Contains("/i/")) return;
                            
                        if (url.Contains("?s="))
                        {
                            url = url.Remove(url.IndexOf("?s="));
                        }

                        if (url.Contains("?t="))
                        {
                            url = url.Remove(url.IndexOf("?t="));
                        }

                        if (!url.Contains("vxtwitter.com") && !url.Contains("fxtwitter.com"))
                        {
                            url = url.Insert(url.IndexOf("twitter.com"), "vx");
                        }

                        if (url == OGurl) return;
                        else
                        {
                            var repost = await message.Channel.SendMessageAsync(url) as IUserMessage;
                            await AddTwitterPost(message, repost);
                            await repost.AddReactionAsync(EmoteHandler.DeletThis);
                        }
                    }
                    else if (url.Contains("https://x.com") || url.Contains("http://x.com"))
                    {
                        if (!url.Contains("/status/") && !url.Contains("/i/")) return;

                        if (url.Contains("?s="))
                        {
                            url = url.Remove(url.IndexOf("?s="));
                        }

                        if (url.Contains("?t="))
                        {
                            url = url.Remove(url.IndexOf("?t="));
                        }

                        if (!url.Contains("vxtwitter.com") && !url.Contains("fxtwitter.com"))
                        {
                            url = url.Replace("x.com", "vxtwitter.com");
                        }

                        if (url == OGurl) return;
                        else
                        {
                            var repost = await message.Channel.SendMessageAsync(url) as IUserMessage;
                            await AddTwitterPost(message, repost);
                            await repost.AddReactionAsync(EmoteHandler.DeletThis);
                        }
                    }
                    else if (url.Contains("fixupx.com") || url.Contains("fixupx.com"))
                    {
                        if (!url.Contains("/status/") && !url.Contains("/i/")) return;

                        if (url.Contains("?s="))
                        {
                            url = url.Remove(url.IndexOf("?s="));
                        }

                        if (url.Contains("?t="))
                        {
                            url = url.Remove(url.IndexOf("?t="));
                        }

                        if (!url.Contains("vxtwitter.com") && !url.Contains("fxtwitter.com"))
                        {
                            url = url.Replace("fixupx.com", "vxtwitter.com");
                        }

                        if (url == OGurl) return;
                        else
                        {
                            var repost = await message.Channel.SendMessageAsync(url) as IUserMessage;
                            await AddTwitterPost(message, repost);
                            await repost.AddReactionAsync(EmoteHandler.DeletThis);
                        }
                    }
                    else return;
                }
                else
                {
                    if (url.Contains("twitter.com") || url.Contains("twitter.com"))
                    {
                        if (url.Contains("ssstwitter") || url.Contains("cdn.discordapp.com")) return;
                        if (!url.Contains("/status/") && !url.Contains("/i/")) return;

                        if (url.Contains("?s="))
                        {
                            url = url.Remove(url.IndexOf("?s="));
                        }

                        if (url.Contains("?t="))
                        {
                            url = url.Remove(url.IndexOf("?t="));
                        }

                        if (!url.Contains("vxtwitter.com") && !url.Contains("fxtwitter.com"))
                        {
                            url = url.Insert(url.IndexOf("twitter.com"), "vx");
                        }

                        if (!url.Contains("https") && url.Contains("http"))
                        {
                            url = url.Replace("http", "https");
                        }

                        if (url == OGurl) return;
                        else
                        {
                            var repost = await message.Channel.SendMessageAsync($"Posted by {message.Author.Username.Replace("_", "\\_").Replace("*", "\\*").Replace("'", "\\'").Replace(">", "\\>").Replace("-", "\\-").Replace("#", "\\#")}: " + url) as IUserMessage;
                            await AddTwitterPost(message, repost);
                            await repost.AddReactionAsync(EmoteHandler.DeletThis);
                            await message.DeleteAsync();
                        }
                    }
                    else if (url.Contains("https://x.com") || url.Contains("http://x.com") && !url.Contains("ssstwitter") && !url.Contains("cdn.discordapp.com"))
                    {
                        if (!url.Contains("/status/") && !url.Contains("/i/")) return;

                        if (url.Contains("?s="))
                        {
                            url = url.Remove(url.IndexOf("?s="));
                        }

                        if (url.Contains("?t="))
                        {
                            url = url.Remove(url.IndexOf("?t="));
                        }

                        if (!url.Contains("https") && url.Contains("http"))
                        {
                            url = url.Replace("http", "https");
                        }

                        if (!url.Contains("vxtwitter.com") && !url.Contains("fxtwitter.com"))
                        {
                            url = url.Replace("x.com", "vxtwitter.com");
                        }

                        if (url == OGurl) return;
                        else
                        {
                            var repost = await message.Channel.SendMessageAsync($"Posted by {message.Author.Username.Replace("_", "\\_").Replace("*", "\\*").Replace("'", "\\'").Replace(">", "\\>").Replace("-", "\\-").Replace("#", "\\#")}: " + url) as IUserMessage;
                            await AddTwitterPost(message, repost);
                            await repost.AddReactionAsync(EmoteHandler.DeletThis);
                            await message.DeleteAsync();
                        }
                    }
                    else if (url.Contains("fixupx.com") || url.Contains("fixupx.com"))
                    {
                        if (!url.Contains("/status/") && !url.Contains("/i/")) return;

                        if (url.Contains("?s="))
                        {
                            url = url.Remove(url.IndexOf("?s="));
                        }

                        if (url.Contains("?t="))
                        {
                            url = url.Remove(url.IndexOf("?t="));
                        }

                        if (!url.Contains("https") && url.Contains("http"))
                        {
                            url = url.Replace("http", "https");
                        }

                        if (!url.Contains("vxtwitter.com") && !url.Contains("fxtwitter.com"))
                        {
                            url = url.Replace("fixupx.com", "vxtwitter.com");
                        }

                        if (url == OGurl) return;
                        else
                        {
                            var repost = await message.Channel.SendMessageAsync($"Posted by {message.Author.Username.Replace("_", "\\_").Replace("*", "\\*").Replace("'", "\\'").Replace(">", "\\>").Replace("-", "\\-").Replace("#", "\\#")}: " + url) as IUserMessage;
                            await AddTwitterPost(message, repost);
                            await repost.AddReactionAsync(EmoteHandler.DeletThis);
                            await message.DeleteAsync();
                        }
                    }
                    else return;
                }
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("COMND: Twitter Posts", null, ex);
            }
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        private async Task AddTwitterPost(IUserMessage post, IUserMessage repost)
        {
            TwitterPost twitterPost = new()
            {
                UserId = post.Author.Id,
                MessageId = repost.Id,
                ChannelId = repost.Channel.Id,
                CreationTime = DateTime.UtcNow
            };
            await db.InsertRecordAsync("TwitterPosts", twitterPost);
        }

        private async Task InstagramReplacer(SocketMessage socketMessage)
        {
            try
            {
                if (socketMessage is not SocketUserMessage message || socketMessage.Author.IsBot) return;

                var forGuildId = socketMessage.Channel as SocketGuildChannel;
                var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", forGuildId.Guild.Id);

                if (serverSettings == null) return;
                if (serverSettings.ReplaceInstagramLinks == null)
                {
                    return;
                }

                if (serverSettings.ReplaceInstagramLinks == "off")
                {
                    return;
                }

                Regex urlRx = new(@"((https?|ftp|file)\://|www.)[A-Za-z0-9\.\-]+(/[A-Za-z0-9\?\&\=;\+!'\(\)\*\-\._~%]*)*", RegexOptions.IgnoreCase);
                string url = urlRx.Match(message.Content).ToString();
                string OGurl = url;

                if (serverSettings.ReplaceInstagramLinks == "reply" || message.Content != url)
                {
                    if (url.Contains("instagram.com"))
                    {
                        if (url.Contains("sssinstagram") || url.Contains("cdn.discordapp.com")) return;
                        if (!url.Contains("/reel/") && !url.Contains("/p/")) return;

                        if (url.Contains("?utm_source=") || url.Contains("&utm_source="))
                        {
                            url = url.Remove(url.IndexOf("utm_source=") - 1);
                        }

                        if (url.Contains("?igsh=") || url.Contains("&igsh="))
                        {
                            url = url.Remove(url.IndexOf("igsh=") - 1);
                        }

                        if (url == OGurl) return;
                        else
                        {
                            var repost = await message.Channel.SendMessageAsync(url) as IUserMessage;
                            await AddInstagramPost(message, repost);
                            await repost.AddReactionAsync(EmoteHandler.DeletThis);
                        }
                    }
                }
                else
                {
                    if (url.Contains("instagram.com"))
                    {
                        if (url.Contains("sssinstagram") || url.Contains("cdn.discordapp.com")) return;
                        if (!url.Contains("/reel/") && !url.Contains("/p/")) return;

                        if (url.Contains("?utm_source=") || url.Contains("&utm_source="))
                        {
                            url = url.Remove(url.IndexOf("utm_source=") - 1);
                        }

                        if (url.Contains("?igsh=") || url.Contains("&igsh="))
                        {
                            url = url.Remove(url.IndexOf("igsh=") - 1);
                        }

                        if (!url.Contains("ddinstagram.com"))
                        {
                            url = url.Insert(url.IndexOf("instagram.com"), "dd");
                        }

                        if (url == OGurl) return;
                        else
                        {
                            var repost = await message.Channel.SendMessageAsync($"Posted by {message.Author.Username.Replace("_", "\\_").Replace("*", "\\*").Replace("'", "\\'").Replace(">", "\\>").Replace("-", "\\-").Replace("#", "\\#")}: " + url) as IUserMessage;
                            await AddInstagramPost(message, repost);
                            await repost.AddReactionAsync(EmoteHandler.DeletThis);
                            await message.DeleteAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("COMND: Instagram Posts", null, ex);
            }
        }

        private async Task AddInstagramPost(IUserMessage post, IUserMessage repost)
        {
            TwitterPost instagramPost = new()
            {
                UserId = post.Author.Id,
                MessageId = repost.Id,
                ChannelId = repost.Channel.Id,
                CreationTime = DateTime.UtcNow
            };
            await db.InsertRecordAsync("InstagramPosts", instagramPost);
        }

        private async Task HeckExMeme(SocketMessage socketMessage)
        {
            if (socketMessage is not SocketUserMessage message || socketMessage.Author.IsBot) return;

            var forGuildId = socketMessage.Channel as SocketGuildChannel;
            if (forGuildId.Guild.Id != 318741497417695234) return;
            if (!message.Content.Contains("my ex")) return;

            await message.ReplyAsync("https://i.imgur.com/Kb2lYGI.png");
        }
    }
}
