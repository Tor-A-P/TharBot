using Discord.Commands;
using org.matheval;
using System.Text.RegularExpressions;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Math : ModuleBase<SocketCommandContext>
    {
        
        [Command("Math")]
        [Alias("calc", "calculate")]
        [Summary("Evaluates a mathematical expression. Refer to https://matheval.org/math-expression-eval-for-c-sharp/ for more details.\n" +
                "**USAGE:** th.math [EXPRESSION]\n" +
                "**EXAMPLE:** th.math  1.2 * (2 + 4.5), th.math sin(45) ^ 2")]
        [Remarks("Utility")]
        public async Task MathAsync([Remainder] string math)
        {
            try
            {
                Expression expression = new(math);
                var value = expression.Eval();

                var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder("Math Calculation");

                var embed = embedBuilder.AddField("Input", $"```{Regex.Replace(math, @"\s+", " ")}```")
                    .AddField("Output", $"```{value}```")
                    .Build();


                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Math", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Math", null, ex);
            }
        }
    }
}
