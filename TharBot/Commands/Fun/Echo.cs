using Discord.Commands;

namespace TharBot.Commands
{
    public class Echo : ModuleBase<SocketCommandContext>
    {
        [Command("Echo")]
        [Alias("Say")]
        [Summary("Repeats any message after the command.\n" +
                "**USAGE:** th.say [MESSAGE]\n" +
                "**EXAMPLE:** th.say I can make the bot say naughty things!")]
        [Remarks("Fun")]
        public async Task SayAsync([Remainder] string echo) => await ReplyAsync(echo);
    }
}
