using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class PollCmd : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly MongoCRUDHandler db;

        public PollCmd(DiscordSocketClient client, IConfiguration config)
        {
            _client = client;
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("Poll")]
        [Alias("MakePoll", "mp")]
        [Summary("Makes a poll with the specified title, description and options, up to 10 options.\n" +
            "**USAGE:** th.Poll [DURATION_IN_MINUTES] \"[TITLE]\" \"[DESCRIPTION]\" [OPTION 1], [OPTION 2], [...]\n" +
            "**EXAMPLE:** th.Poll 300 \"What kind of fish do you like?\" \"For food, not for pet\" Cod, Salmon, Pike, Herring\n" +
            "The example above will report its results after 300 minutes, aka 5 hours")]
        [Remarks("Utility")]
        public async Task PollAsync(int duration, string title, string description, [Remainder] string choices)
        {
            var options = choices.Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (options.Length > 10)
            {
                var manyOptionsEmbed = await EmbedHandler.CreateUserErrorEmbed("Poll", "Too many options! You can only add up to 10 options.");
                await ReplyAsync(embed: manyOptionsEmbed);
            }
            else
            {
                description = description.Trim() + "\n\n";
                Emoji[] emojis =
                {
                new Emoji("1️⃣"),
                new Emoji("2️⃣"),
                new Emoji("3️⃣"),
                new Emoji("4️⃣"),
                new Emoji("5️⃣"),
                new Emoji("6️⃣"),
                new Emoji("7️⃣"),
                new Emoji("8️⃣"),
                new Emoji("9️⃣"),
                new Emoji("🔟")
                };

                var emojinames = new List<string>();

                foreach (var emoji in emojis)
                {
                    emojinames.Add(emoji.Name);
                }

                for (int i = 0; i < options.Length; i++)
                {
                    description = description + emojis[i] + ": " + options[i].Trim() + "\n";
                }

                var embed = await EmbedHandler.CreateBasicEmbed(title, description);

                var poll = await ReplyAsync(embed: embed);

                var serverSpecifics = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
                if (serverSpecifics.Polls != null)
                {
                    var activePoll = serverSpecifics.Polls.Where(x => x.MessageId == poll.Id).FirstOrDefault();
                    if (activePoll != null)
                    {
                        var alreadyExistsEmbed = await EmbedHandler.CreateErrorEmbed("Poll", $"Something went horribly wrong, deleting Poll!\n" +
                            $"Please send a DM to Tharwatha#5189 with the message \"Poll with ID {poll.Id} failed because it was a duplicate\" so I can investigate");
                        await ReplyAsync(embed: alreadyExistsEmbed);
                        await poll.DeleteAsync();
                        return;
                    }
                }

                try
                {
                    var newPoll = new Poll
                    {
                        MessageId = poll.Id,
                        ChannelId = poll.Channel.Id,
                        Emojis = emojinames,
                        Responses = new List<ActivePollResponse>(),
                        CreationTime = DateTime.UtcNow,
                        LifeSpan = TimeSpan.FromMinutes((double)duration),
                        CompletionTime = DateTime.UtcNow + TimeSpan.FromMinutes((double)duration),
                        NumOptions = options.Length
                    };
                    serverSpecifics.Polls.Add(newPoll);
                    var update = Builders<ServerSpecifics>.Update.Set(x => x.Polls, serverSpecifics.Polls);
                    await db.UpsertServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);

                    for (int i = 0; i < options.Length; i++)
                    {
                        await poll.AddReactionAsync(emojis[i]);
                    }
                }
                catch (Exception ex)
                {
                    var exEmbed = await EmbedHandler.CreateErrorEmbed("Poll", ex.Message);
                    await ReplyAsync(embed: exEmbed);
                    await LoggingHandler.LogCriticalAsync("COMND: Poll", null, ex);
                }
            }
        }
    }
}
