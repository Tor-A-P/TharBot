﻿using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Timers;
using TharBot.DBModels;

namespace TharBot.Handlers
{
    public class ScheduledEventsHandler : DiscordClientService
    {
        private static readonly System.Timers.Timer timer25s = new(25000);
        private static readonly System.Timers.Timer timer60s = new(60000);
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;

        public ScheduledEventsHandler(DiscordSocketClient client, IConfiguration configuration, ILogger<DiscordClientService> logger)
            : base(client, logger)
        {
            _client = client;
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", _configuration);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            timer25s.Enabled = true;
            timer25s.Elapsed += PollHandling;
            timer25s.Elapsed += ReminderHandling;
            timer25s.Elapsed += DailyPCHandling;

            timer60s.Enabled = true;
            timer60s.Elapsed += GameHandling;
            timer60s.Elapsed += FightOverHandling;
            timer60s.Elapsed += AttributeDialogCleanup;
        }

        public async void PollHandling(object? source, ElapsedEventArgs e)
        {
            var serverSpecifics = db.LoadRecords<ServerSpecifics>("ServerSpecifics");

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
                                var resultsChannelSettings = db.LoadRecordById<ServerSpecifics>("ServerSpecifics", forGuildId.Guild.Id).PCResultsChannel;
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

        public void RemovePoll(ServerSpecifics? server, Poll? poll)
        {
            server = db.LoadRecordById<ServerSpecifics>("ServerSpecifics", server.ServerId);
            poll = server.Polls.Where(x => x.MessageId == poll.MessageId).FirstOrDefault();
            server.Polls.Remove(poll);
            db.UpsertRecord("ServerSpecifics", server.ServerId, server);
        }

        public async void ReminderHandling(object? source, ElapsedEventArgs e)
        {
            try
            {
                var serverSpecifics = db.LoadRecords<ServerSpecifics>("ServerSpecifics");

                foreach (var server in serverSpecifics)
                {
                    foreach (var reminder in server.Reminders)
                    {
                        if (reminder.RemindingTime < DateTime.UtcNow)
                        {
                            var channel = await Client.GetChannelAsync(reminder.ChannelId) as IMessageChannel;
                            var user = await Client.GetUserAsync(reminder.UserId);

                            server.Reminders.Remove(reminder);
                            db.UpsertRecord("ServerSpecifics", server.ServerId, server);
                            await channel.SendMessageAsync($"Reminder, {user.Mention}: {reminder.ReminderText}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("Bot", null, ex);
            }
        }

        public async void DailyPCHandling(object? source, ElapsedEventArgs e)
        {
            var serverSpecifics = db.LoadRecords<ServerSpecifics>("ServerSpecifics");
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
                            db.UpsertRecord("ServerSpecifics", server.ServerId, server);

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
            var userProfiles = db.LoadRecords<GameUser>("UserProfiles");

            if (userProfiles == null) return;

            foreach (var userProfile in userProfiles)
            {
                foreach (var serverStats in userProfile.Servers)
                {
                    var percentageHealthRegen = (serverStats.Attributes.Constitution * GameServerStats.ConstitutionHPRegenBonus) + 5;
                    var percentageManaRegen = (serverStats.Attributes.Wisdom * GameServerStats.WisdomMPRegenBonus) + 5;
                    serverStats.CurrentHP += Math.Floor(serverStats.BaseHP / 100 * percentageHealthRegen);
                    serverStats.CurrentMP += Math.Floor(serverStats.BaseMP / 100 * percentageManaRegen);
                    if (serverStats.CurrentHP > serverStats.BaseHP) serverStats.CurrentHP = serverStats.BaseHP;
                    if (serverStats.CurrentMP > serverStats.BaseMP) serverStats.CurrentMP = serverStats.BaseMP;
                }
                db.UpsertRecord("UserProfiles", userProfile.UserId, userProfile);
            }
        }

        public async void FightOverHandling(object? source, ElapsedEventArgs e)
        {
            var activeFights = db.LoadRecords<GameFight>("ActiveFights");

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
                                var userProfile = db.LoadRecordById<GameUser>("UserProfiles", fight.UserId);
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
                                db.DeleteRecord<GameFight>("ActiveFights", fight.MessageId);
                                db.UpsertRecord("UserProfiles", userProfile.UserId, userProfile);
                            }
                        }
                    }
                }
            }
        }

        public async void AttributeDialogCleanup(object? source, ElapsedEventArgs e)
        {
            try
            {
                var serverSpecifics = db.LoadRecords<ServerSpecifics>("ServerSpecifics");

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
                                server.AttributeDialogs.Remove(dialog);
                                db.UpsertRecord("ServerSpecifics", server.ServerId, server);
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
    }
}