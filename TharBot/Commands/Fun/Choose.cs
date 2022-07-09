using Discord.Commands;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Choose : ModuleBase<SocketCommandContext>
    {
        [Command("Choose")]
        [Alias("Choice")]
        [Summary("Chooses between two or more arguments supplied, separated by comma.\n" +
                "**USAGE:** th.choose [CHOICE_A],[CHOICE_B],[...]\n" +
                "**EXAMPLE:** th.choose yes, no, maybe")]
        [Remarks("Fun")]
        public async Task ChooseAsync([Remainder] string msg)
        {
            Random random = new();

            if (msg.Split(',').Length < 2)
            {
                var noChoiceEmbed = await EmbedHandler.CreateUserErrorEmbed("Not enough choices", "Please enter two or more choices, separated by comma");
                await ReplyAsync(embed: noChoiceEmbed);
            }
            else
            {
                var embed = await EmbedHandler.CreateBasicEmbed("I choose:", $"```{msg.Split(',')[random.Next(msg.Split(',').Length)].Trim()}```");

                await ReplyAsync(embed: embed);
            }
        }
    }
}
