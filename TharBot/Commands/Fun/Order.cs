using Discord.Commands;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Order : ModuleBase<SocketCommandContext>
    {
        [Command("Order")]
        [Alias("Randomize")]
        [Summary("Randomly reorders two or more given choices, separated by comma.\n" +
                "**USAGE:** th.order [CHOICE_A, [CHOICE_B], [...]\n" +
                "**EXAMPLE:** th.order apple, banana, kiwi")]
        [Remarks("Fun")]
        public async Task OrderAsync([Remainder] string msg)
        {
            Random random = new();
            string result = "";
            msg = msg.Replace(" ", "");
            var arr = msg.Split(',').OrderBy(x => random.Next()).ToArray();

            if (arr.Length < 2)
            {
                var noChoiceEmbed = await EmbedHandler.CreateUserErrorEmbed("Not enough choices", "Please enter two or more choices, separated by comma");
                await ReplyAsync(embed: noChoiceEmbed);
            }
            else
            {
                foreach (var choice in arr)
                {
                    result += choice + ", ";
                }
                result = result.Remove(result.Length - 2, 2);

                await ReplyAsync(result);
            }
        }
    }
}
