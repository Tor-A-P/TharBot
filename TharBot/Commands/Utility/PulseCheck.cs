using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class PulseCheck : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly MongoCRUDHandler db;

        public PulseCheck(DiscordSocketClient client, IConfiguration config)
        {
            _client = client;
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("PulseCheck")]
        [Alias("pc")]
        [Summary("Creates a message for a \"pulse check\", e.g. a message users can react to to indicate their mood today, with an optionally specified duration." +
            "No duration specified results in a duration of 6 hours.\n" +
            "**USAGE:** th.PulseCheck, th.PulseCheck [DURATION_IN_MINUTES]\n" +
            "**EXAMPLE:** th.PulseCheck 300")]
        [Remarks("Utility")]
        public async Task PulseCheckAsync(int? duration = null)
        {
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

            var pulsecheck = await ReplyAsync(embed: embed);

            var serverSpecifics = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
            if (serverSpecifics.Polls != null)
            {
                var activePoll = serverSpecifics.Polls.Where(x => x.MessageId == pulsecheck.Id).FirstOrDefault();
                if (activePoll != null)
                {
                    var alreadyExistsEmbed = await EmbedHandler.CreateErrorEmbed("PulseCheck", $"Something went horribly wrong, deleting Poll!\n" +
                        $"Please send a DM to Tharwatha#5189 with the message \"PulseCheck with ID {pulsecheck.Id} failed because it was a duplicate\" so I can investigate");
                    await ReplyAsync(embed: alreadyExistsEmbed);
                    await pulsecheck.DeleteAsync();
                    return;
                }
            }

            try
            {
                if (duration == null) duration = 360;
                var newPoll = new Poll
                {
                    MessageId = pulsecheck.Id,
                    ChannelId = pulsecheck.Channel.Id,
                    Emojis = emojinames,
                    Responses = new List<ActivePollResponse>(),
                    CreationTime = DateTime.UtcNow,
                    LifeSpan = TimeSpan.FromMinutes((double)duration),
                    CompletionTime = DateTime.UtcNow + TimeSpan.FromMinutes((double)duration)
                };
                serverSpecifics.Polls.Add(newPoll);
                await db.UpsertRecordAsync("ServerSpecifics", Context.Guild.Id, serverSpecifics);

                foreach (var emoji in emojis)
                {
                    await pulsecheck.AddReactionAsync(emoji);
                }

                //await Task.Delay(duration.Value * 60000);

                //var movePoll = await db.LoadRecordByIdAsync<Poll>("ActivePolls", newPoll.MessageId);
                //db.InsertRecord("InactivePolls", movePoll);

                //int[] resultsCount =
                //{
                //    0, 0, 0, 0, 0, 0
                //};

                //foreach (var vote in movePoll.Responses)
                //{
                //    switch (vote.Vote)
                //    {
                //        case "😀":
                //            resultsCount[0]++;
                //            break;
                //        case "🙂":
                //            resultsCount[1]++;
                //            break;
                //        case "😐":
                //            resultsCount[2]++;
                //            break;
                //        case "☹":
                //            resultsCount[3]++;
                //            break;
                //        case "😢":
                //            resultsCount[4]++;
                //            break;
                //        case "😡":
                //            resultsCount[5]++;
                //            break;
                //        default:
                //            break;
                //    }
                //}

                //var resultsEmbed = await EmbedHandler.CreateBasicEmbedBuilder("Results from pulsecheck command:");
                //resultsEmbed = resultsEmbed
                //               .AddField("😀 answers:", resultsCount[0])
                //               .AddField("🙂 answers:", resultsCount[1])
                //               .AddField("😐 answers:", resultsCount[2])
                //               .AddField("☹ answers:", resultsCount[3])
                //               .AddField("😢 answers:", resultsCount[4])
                //               .AddField("😡 answers:", resultsCount[5]);

                //var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
                //if (serverSettings.PCResultsChannel != null)
                //{
                //    var chan = await _client.GetChannelAsync((ulong)serverSettings.PCResultsChannel) as IMessageChannel;
                //    await chan.SendMessageAsync(embed: resultsEmbed.Build());
                //}
                //else
                //{
                //    var chan = await _client.GetChannelAsync(newPoll.ChannelId) as IMessageChannel;
                //    await chan.SendMessageAsync(embed: resultsEmbed.Build());
                //}

                //await db.DeleteRecordAsync<Poll>("ActivePolls", newPoll.MessageId);

                //var getChannel = await _client.GetChannelAsync(newPoll.ChannelId) as IMessageChannel;
                //var msg = await getChannel.GetMessageAsync(newPoll.MessageId);
                //if (msg.Channel.GetMessageAsync(msg.Id) != null)
                //{
                //    await msg.DeleteAsync();
                //}
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("PulseCheck", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: PulseCheck", null, ex);
            }
        }
    }
}
