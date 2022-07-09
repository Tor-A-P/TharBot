using Discord.Commands;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class EightBall : ModuleBase<SocketCommandContext>
    {
        [Command("8ball")]
        [Alias("eightball", "question")]
        [Summary("Asks the bot a question, answered with classic eight-ball answers.\n" +
                "**USAGE:** th.8ball [QUESTION]\n" +
                "**EXAMPLE:** th.8ball Will I ever be the little girl?")]
        [Remarks("Fun")]
        public async Task EightBallAsync([Remainder] string question)
        {

            var replies = new List<string>
            {
                "It is certain.",
                "It is decidedly so.",
                "Without a doubt.",
                "Yes - definitely.",
                "You may rely on it.",
                "As I see it, yes.",
                "Most likely.",
                "Outlook good.",
                "Yes.",
                "Signs point to yes.",
                "Reply hazy, try again.",
                "Ask again later.",
                "Better not tell you now.",
                "Cannot predict now.",
                "Concentrate and ask again.",
                "Don't count on it.",
                "My reply is no.",
                "My sources say no.",
                "Outlook not so good.",
                "Very doubtful."
            };

            var answer = replies[new Random().Next(replies.Count)];

            var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder("Magic Eight-Ball");

            var embed = embedBuilder.AddField("Your question", question)
                .AddField("My divine answer", answer)
                .WithThumbnailUrl("https://i.imgur.com/Yq1NlTX.png")
                .Build();                

            await ReplyAsync(embed: embed);
        }
    }
}
