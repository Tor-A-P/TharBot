using Discord.Commands;
using DnDGen.RollGen.IoC;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class DiceRoll : ModuleBase<SocketCommandContext>
    {
        [Command("Roll")]
        [Alias("Dice")]
        [Summary("Rolls a set of dice, if no dice specified, rolls 1-100.\n" +
                "**USAGE:** th.roll, th.roll [STANDARD_DICE], th.roll [STANDARD_DICE] + [MODIFIER]\n" +
                "**EXAMPLE:** th.roll 20, th.roll 3d6, th.roll 1d20 + 3\nSee https://github.com/DnDGen/RollGen for more expressions.")]
        [Remarks("Fun")]
        public async Task DiceRollAsync([Remainder] string expression = "")
        {
            Random random = new();
            int result;

            try
            {
                if (expression == "")
                {
                    result = random.Next(1, 101);
                    expression = "Random 1-100";
                }
                else if (int.TryParse(expression, out int num))
                {
                    result = random.Next(1, num);
                    expression = $"Random 1-{expression}";
                }
                else
                {
                    var dice = DiceFactory.Create();
                    result = dice.Roll(expression).AsSum();
                }

                var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder("Dice Roll");

                var embed = embedBuilder
                            .AddField("Input", $"```{expression}```")
                            .AddField("Result of dice roll", $"```{result}```")
                            .WithThumbnailUrl("https://i.imgur.com/ut3Cyin.png")
                            .Build();

                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Roll", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Roll", null, ex);
            }
        }
    }
}
