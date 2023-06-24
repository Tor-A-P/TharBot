using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Timers;
using TharBot.DBModels;
using Victoria.Node;
using Victoria.Player;

namespace TharBot.Handlers
{
    public class ScheduledEventsHandler : DiscordClientService
    {
        private static readonly System.Timers.Timer timer25s = new(25000);
        private static readonly System.Timers.Timer timer60s = new(60000);
        private static readonly System.Timers.Timer timer300s = new(300000);
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;
        private readonly LavaNode _lavaNode;

        public ScheduledEventsHandler(DiscordSocketClient client, IConfiguration configuration, ILogger<DiscordClientService> logger, LavaNode lavaNode)
            : base(client, logger)
        {
            _client = client;
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", _configuration);
            _lavaNode = lavaNode;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            timer25s.Enabled = true;
            timer25s.Elapsed += PollHandling;
            timer25s.Elapsed += ReminderHandling;
            timer25s.Elapsed += DailyPCHandling;
            timer25s.Elapsed += StopMusic;

            timer60s.Enabled = true;
            timer60s.Elapsed += GameHandling;
            timer60s.Elapsed += FightOverHandling;
            timer60s.Elapsed += AttributeDialogCleanup;

            timer300s.Enabled = true;
            timer300s.Elapsed += AvatarChanging;
        }

        public async void PollHandling(object? source, ElapsedEventArgs e)
        {
            var serverSpecifics = await db.LoadRecordsAsync<ServerSpecifics>("ServerSpecifics");
            if (serverSpecifics == null) return;

            foreach (var server in serverSpecifics)
            {
                if (server.Polls == null) break;
                var pollRecs = server.Polls;

                foreach (var poll in pollRecs)
                {
                    if (poll.CompletionTime < DateTime.UtcNow)
                    {
                        if (poll.Emojis.Contains("😀"))
                        {
                            try
                            {
                                int[] resultsCount =
                                {
                                0, 0, 0, 0, 0, 0
                            };

                                foreach (var vote in poll.Responses)
                                {
                                    switch (vote.Vote)
                                    {
                                        case "😀":
                                            resultsCount[0]++;
                                            break;
                                        case "🙂":
                                            resultsCount[1]++;
                                            break;
                                        case "😐":
                                            resultsCount[2]++;
                                            break;
                                        case "☹":
                                            resultsCount[3]++;
                                            break;
                                        case "😢":
                                            resultsCount[4]++;
                                            break;
                                        case "😡":
                                            resultsCount[5]++;
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                var resultsEmbed = await EmbedHandler.CreateBasicEmbedBuilder("Results from pulsecheck command:");
                                resultsEmbed = resultsEmbed
                                               .AddField("😀 answers:", resultsCount[0])
                                               .AddField("🙂 answers:", resultsCount[1])
                                               .AddField("😐 answers:", resultsCount[2])
                                               .AddField("☹ answers:", resultsCount[3])
                                               .AddField("😢 answers:", resultsCount[4])
                                               .AddField("😡 answers:", resultsCount[5]);

                                var forGuildId = await Client.GetChannelAsync(poll.ChannelId) as SocketGuildChannel;
                                var resultsChannelSettings = (await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", forGuildId.Guild.Id)).PCResultsChannel;
                                if (resultsChannelSettings != null)
                                {
                                    var chan = await Client.GetChannelAsync((ulong)resultsChannelSettings) as IMessageChannel;
                                    await chan.SendMessageAsync(embed: resultsEmbed.Build());
                                }
                                else
                                {
                                    var chan = await Client.GetChannelAsync(poll.ChannelId) as IMessageChannel;
                                    await chan.SendMessageAsync(embed: resultsEmbed.Build());
                                }

                                RemovePoll(server, poll);

                                var getChannel = await Client.GetChannelAsync(poll.ChannelId) as IMessageChannel;
                                var msg = await getChannel.GetMessageAsync(poll.MessageId);
                                if (msg != null)
                                {
                                    if (msg.Channel.GetMessageAsync(msg.Id) != null)
                                    {
                                        await msg.DeleteAsync();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                await LoggingHandler.LogCriticalAsync("Bot", null, ex);
                            }
                        }
                        else if (poll.Emojis.Contains("1️⃣"))
                        {
                            try
                            {
                                int[] resultsCount =
                                {
                                0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                            };

                                foreach (var vote in poll.Responses)
                                {
                                    switch (vote.Vote)
                                    {
                                        case "1️⃣":
                                            resultsCount[0]++;
                                            break;
                                        case "2️⃣":
                                            resultsCount[1]++;
                                            break;
                                        case "3️⃣":
                                            resultsCount[2]++;
                                            break;
                                        case "4️⃣":
                                            resultsCount[3]++;
                                            break;
                                        case "5️⃣":
                                            resultsCount[4]++;
                                            break;
                                        case "6️⃣":
                                            resultsCount[5]++;
                                            break;
                                        case "7️⃣":
                                            resultsCount[6]++;
                                            break;
                                        case "8️⃣":
                                            resultsCount[7]++;
                                            break;
                                        case "9️⃣":
                                            resultsCount[8]++;
                                            break;
                                        case "🔟":
                                            resultsCount[9]++;
                                            break;
                                        default:
                                            break;
                                    }
                                }

                                var forGuildId = await Client.GetChannelAsync(poll.ChannelId) as SocketGuildChannel;
                                var resultsEmbed = await EmbedHandler.CreateBasicEmbedBuilder("Results from poll:");

                                for (int i = 0; i < poll.NumOptions; i++)
                                {
                                    resultsEmbed = resultsEmbed
                                                   .AddField($"{poll.Emojis[i]} votes:", resultsCount[i]);
                                }

                                var pollChn = await Client.GetChannelAsync(poll.ChannelId) as IMessageChannel;
                                var pollMsg = await pollChn.GetMessageAsync(poll.MessageId);
                                if (pollMsg != null)
                                {
                                    var pollEmbed = pollMsg.Embeds.FirstOrDefault();
                                    var winner = pollEmbed.Description.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                                    int highestCount = resultsCount.Max();
                                    int winnerNum = Array.IndexOf(resultsCount, highestCount) + 1;

                                    resultsEmbed = resultsEmbed.AddField("AND THE WINNER IS", $"With {highestCount} votes,\n" +
                                        $"{winner[winnerNum]}! 🎉")
                                                   .WithUrl($"https://discord.com/channels/{forGuildId.Guild.Id}/{forGuildId.Id}/{poll.MessageId}");
                                    await pollMsg.RemoveAllReactionsAsync();
                                    await pollChn.SendMessageAsync(embed: resultsEmbed.Build());
                                }
                                else
                                {
                                    int highestCount = resultsCount.Max();
                                    int winnerNum = Array.IndexOf(resultsCount, highestCount);
                                    resultsEmbed = resultsEmbed.AddField("AND THE WINNER IS", $"With {highestCount} votes,\n" +
                                        $"{poll.Emojis[winnerNum]}! 🎉");

                                    await pollChn.SendMessageAsync(embed: resultsEmbed.Build());
                                }

                                RemovePoll(server, poll);
                            }
                            catch (Exception ex)
                            {
                                await LoggingHandler.LogCriticalAsync("bot", null, ex);
                            }
                        }
                    }
                }
            }
        }

        public async void RemovePoll(ServerSpecifics? server, Poll? poll)
        {
            server = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", server.ServerId);
            if (server == null) return;
            poll = server.Polls.Where(x => x.MessageId == poll.MessageId).FirstOrDefault();
            server.Polls.Remove(poll);
            var update = Builders<ServerSpecifics>.Update.Set(x => x.Polls, server.Polls);
            await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", server.ServerId, update);
        }

        public async void ReminderHandling(object? source, ElapsedEventArgs e)
        {
            try
            {
                var serverSpecifics = await db.LoadRecordsAsync<ServerSpecifics>("ServerSpecifics");
                if (serverSpecifics == null) return;

                foreach (var server in serverSpecifics)
                {
                    foreach (var reminder in server.Reminders)
                    {
                        if (reminder.RemindingTime < DateTime.UtcNow)
                        {
                            var channel = await Client.GetChannelAsync(reminder.ChannelId) as IMessageChannel;
                            var user = await Client.GetUserAsync(reminder.UserId);

                            await channel.SendMessageAsync($"Reminder, {user.Mention}: {reminder.ReminderText}");
                            RemoveReminder(server, reminder);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("Bot", null, ex);
            }
        }

        public async void RemoveReminder(ServerSpecifics? server, Reminders? reminder)
        {
            server = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", server.ServerId);
            if (server == null) return;
            reminder = server.Reminders.Where(x => x.Id == reminder.Id).FirstOrDefault();
            server.Reminders.Remove(reminder);
            var update = Builders<ServerSpecifics>.Update.Set(x => x.Reminders, server.Reminders);
            await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", server.ServerId, update);
        }

        public async void DailyPCHandling(object? source, ElapsedEventArgs e)
        {
            var serverSpecifics = await db.LoadRecordsAsync<ServerSpecifics>("ServerSpecifics");
            if (serverSpecifics == null) return;
            foreach (var server in serverSpecifics)
            {
                if (server.DailyPC == null) return;
                if (!server.DailyPC.OnWeekends)
                {
                    var day = DateTime.UtcNow.DayOfWeek;
                    if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) return;
                }
                if (server.DailyPC.LastTimeRun.Date < DateTime.UtcNow.Date)
                {
                    if (TimeOnly.FromDateTime(server.DailyPC.WhenToRun) < TimeOnly.FromDateTime(DateTime.UtcNow))
                    {
                        try
                        {
                            var channel = await Client.GetChannelAsync(server.DailyPC.ChannelId) as IMessageChannel;
                            Emoji[] emojis =
                            {
                            new Emoji("😀"),
                            new Emoji("🙂"),
                            new Emoji("😐"),
                            new Emoji("☹"),
                            new Emoji("😢"),
                            new Emoji("😡")
                            };

                            var emojinames = new List<string>();

                            foreach (var emoji in emojis)
                            {
                                emojinames.Add(emoji.Name);
                            }

                            var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder("Pulse Check");

                            var embed = embedBuilder
                                .AddField("How are you feeling today?", "React to the emojis below to indicate your mood today!")
                                .WithCurrentTimestamp()
                                .Build();

                            if (server.DailyPC.ShouldPing)
                            {
                                await channel.SendMessageAsync("@here, please answer today's pulsecheck!");
                            }
                            var pulsecheck = await channel.SendMessageAsync(embed: embed);

                            var newPoll = new Poll
                            {
                                MessageId = pulsecheck.Id,
                                ChannelId = pulsecheck.Channel.Id,
                                Emojis = emojinames,
                                Responses = new List<ActivePollResponse>(),
                                CreationTime = DateTime.UtcNow,
                                LifeSpan = TimeSpan.FromMinutes(server.DailyPC.Duration),
                                CompletionTime = DateTime.UtcNow + TimeSpan.FromMinutes(server.DailyPC.Duration)
                            };
                            server.Polls.Add(newPoll);
                            server.DailyPC.LastTimeRun = DateTime.UtcNow;
                            var update = Builders<ServerSpecifics>.Update
                                .Set(x => x.Polls, server.Polls)
                                .Set(x => x.DailyPC, server.DailyPC);
                            await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", server.ServerId, update);

                            foreach (var emoji in emojis)
                            {
                                await pulsecheck.AddReactionAsync(emoji);
                            }
                        }
                        catch (Exception ex)
                        {
                            await LoggingHandler.LogCriticalAsync("Bot", null, ex);
                            return;
                        }
                    }
                }
            }
        }

        public async void GameHandling(object? source, ElapsedEventArgs e)
        {
            try
            {
                var userProfiles = await db.LoadRecordsAsync<GameUser>("UserProfiles");
                Random random = new();

                if (userProfiles == null) return;

                foreach (var userProfile in userProfiles)
                {
                    foreach (var serverStats in userProfile.Servers)
                    {
                        var guild = Client.GetGuild(serverStats.ServerId);
                        if (guild != null)
                        {
                            var user = guild.GetUser(userProfile.UserId);
                            if (user != null)
                            {
                                if (user.VoiceChannel != null)
                                {
                                    serverStats.Exp += random.Next(8, 13);
                                    serverStats.TharCoins += 10;

                                    if (serverStats.Exp >= serverStats.ExpToLevel)
                                    {
                                        serverStats.Exp -= serverStats.ExpToLevel;
                                        serverStats.Level++;
                                        serverStats.CurrentHP = serverStats.BaseHP;
                                        serverStats.CurrentMP = serverStats.BaseMP;
                                        serverStats.AttributePoints += GameServerStats.AttributePointsPerLevel;
                                    }
                                }
                            }
                        }


                        var percentageHealthRegen = (serverStats.Attributes.Constitution * GameServerStats.ConstitutionHPRegenBonus) + 5;
                        var percentageManaRegen = (serverStats.Attributes.Wisdom * GameServerStats.WisdomMPRegenBonus) + 5;
                        serverStats.CurrentHP += Math.Floor(serverStats.BaseHP / 100 * percentageHealthRegen);
                        serverStats.CurrentMP += Math.Floor(serverStats.BaseMP / 100 * percentageManaRegen);
                        if (serverStats.CurrentHP > serverStats.BaseHP) serverStats.CurrentHP = serverStats.BaseHP;
                        if (serverStats.CurrentMP > serverStats.BaseMP) serverStats.CurrentMP = serverStats.BaseMP;
                    }
                    var update = Builders<GameUser>.Update.Set(x => x.Servers, userProfile.Servers);
                    await db.UpdateUserAsync<GameUser>("UserProfiles", userProfile.UserId, update);
                }
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("Bot", null, ex);
                return;
            }
            
        }

        public async void FightOverHandling(object? source, ElapsedEventArgs e)
        {
            try
            {
                var activeFights = await db.LoadRecordsAsync<GameFight>("ActiveFights");
                if (activeFights == null) return;

                foreach (var fight in activeFights)
                {
                    if (fight.LastMoveTime + GameFight.LifeTime < DateTime.UtcNow)
                    {
                        if (await Client.GetChannelAsync(fight.ChannelId) is IMessageChannel chn)
                        {
                            if (await chn.GetMessageAsync(fight.MessageId) is IUserMessage msg)
                            {
                                var embed = msg.Embeds.FirstOrDefault();
                                if (embed != null)
                                {
                                    var userProfile = await db.LoadRecordByIdAsync<GameUser>("UserProfiles", fight.UserId);
                                    var serverStats = userProfile.Servers.Where(x => x.ServerId == fight.ServerId).FirstOrDefault();
                                    var user = await Client.GetUserAsync(fight.UserId);
                                    var builder = embed.ToEmbedBuilder();
                                    builder.AddField($"{user.Username} ran away from the battle!", "Nobody wins this battle.");
                                    await msg.ModifyAsync(x => x.Embed = builder.Build());
                                    await msg.RemoveAllReactionsAsync();
                                    serverStats.FightInProgress = false;
                                    serverStats.Debuffs = new GameDebuffs
                                    {
                                        StunDuration = 0,
                                        HoTDuration = 0,
                                        HoTStrength = 0,
                                        DoTDuration = 0,
                                        DoTStrength = 0
                                    };
                                    await db.DeleteRecordAsync<GameFight>("ActiveFights", fight.MessageId);
                                    var update = Builders<GameUser>.Update.Set(x => x.Servers, userProfile.Servers);
                                    await db.UpdateUserAsync<GameUser>("UserProfiles", userProfile.UserId, update);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("Bot", null, ex);
                return;
            }
        }

        public async void AttributeDialogCleanup(object? source, ElapsedEventArgs e)
        {
            try
            {
                var serverSpecifics = await db.LoadRecordsAsync<ServerSpecifics>("ServerSpecifics");
                if (serverSpecifics == null) return;

                foreach (var server in serverSpecifics)
                {
                    var attributeDialogs = server.AttributeDialogs;
                    foreach (var dialog in attributeDialogs)
                    {
                        try
                        {
                            if (dialog.CreationTime + GameAttributeDialog.LifeTime < DateTime.UtcNow)
                            {
                                if (await Client.GetChannelAsync(dialog.ChannelId) is IMessageChannel chn)
                                {
                                    var msg = await chn.GetMessageAsync(dialog.MessageId);
                                    if (msg != null)
                                    {
                                        await msg.RemoveAllReactionsAsync();
                                    }
                                }
                                RemoveAttributeDialog(server, dialog);
                            }
                        }
                        catch (Exception ex)
                        {
                            await LoggingHandler.LogCriticalAsync("Bot", null, ex);
                            continue;
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("Bot", null, ex);
            }
        }

        public async void RemoveAttributeDialog(ServerSpecifics? server, GameAttributeDialog? attributeDialog)
        {
            server = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", server.ServerId);
            if (server == null) return;
            attributeDialog = server.AttributeDialogs.Where(x => x.MessageId == attributeDialog.MessageId).FirstOrDefault();
            server.AttributeDialogs.Remove(attributeDialog);
            var update = Builders<ServerSpecifics>.Update.Set(x => x.AttributeDialogs, server.AttributeDialogs);
            await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", server.ServerId, update);
        }

        public async void StopMusic(object? source, ElapsedEventArgs e)
        {
            try
            {
                var serverSpecifics = await db.LoadRecordsAsync<ServerSpecifics>("ServerSpecifics");
                if (serverSpecifics == null) return;
                foreach (var server in serverSpecifics)
                {
                    var guild = _client.GetGuild(server.ServerId);
                    if (guild == null) continue;

                    var bot = guild.GetUser(_client.CurrentUser.Id);
                    if (bot == null) continue;

                    var voiceChannel = bot.VoiceChannel;
                    if (voiceChannel == null)
                    {
                        if (_lavaNode.TryGetPlayer(guild, out var player))
                        {
                            if (player.VoiceChannel != null)
                            {
                                if (player.PlayerState is PlayerState.Playing)
                                {
                                    player.Vueue.Clear();
                                    await player.StopAsync();

                                    await _lavaNode.LeaveAsync(player.VoiceChannel);
                                }
                                else
                                {
                                    await player.StopAsync();
                                    await _lavaNode.LeaveAsync(player.VoiceChannel);
                                }
                            }
                        }
                        continue;
                    }

                    if (voiceChannel.ConnectedUsers.Count == 1)
                    {
                        if (_lavaNode.HasPlayer(guild))
                        {
                            _lavaNode.TryGetPlayer(guild, out var player);

                            if (player.PlayerState is PlayerState.Playing)
                            {
                                player.Vueue.Clear();
                                await player.StopAsync();

                                await _lavaNode.LeaveAsync(voiceChannel);
                            }
                            else
                            {
                                await player.StopAsync();
                                await _lavaNode.LeaveAsync(voiceChannel);
                            }

                            var embed = await EmbedHandler.CreateBasicEmbed("Left voice chat and stopped player", "I'm not gonna play music for only myself! >:(");
                            var channel = await _client.GetChannelAsync(server.LastChannelUsedId) as IMessageChannel;
                            await channel.SendMessageAsync(embed: embed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("bot", null, ex);
            }
        }

        public async void AvatarChanging(object? source, ElapsedEventArgs e)
        {
            try
            {
                Random random = new();
                var fileStream = new FileStream(Directory.GetCurrentDirectory() + "/Avatars/Avatar" + random.Next(0, 5) + ".png", FileMode.Open);
                var image = new Image(fileStream);
                await Client.CurrentUser.ModifyAsync(u => u.Avatar = image);
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("bot", null, ex);
            }
            
        }
    }
}