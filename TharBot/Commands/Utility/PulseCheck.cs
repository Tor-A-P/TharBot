using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
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
                var update = Builders<ServerSpecifics>.Update.Set(x => x.Polls, serverSpecifics.Polls);
                await db.UpdateServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);

                foreach (var emoji in emojis)
                {
                    await pulsecheck.AddReactionAsync(emoji);
                }
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
