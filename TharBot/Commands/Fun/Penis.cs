using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TharBot.Commands.Fun
{
    public class Penis : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;

        public Penis(DiscordSocketClient client)
        {
            _client = client;
        }

        [Command("Penis")]
        [Alias("Dick", "Cock", "Peener", "Benis")]
        [Summary("Shows a user's penis size.\n" +
                "**USAGE:** th.penis [OPTIONAL_USER]\n" +
                "**EXAMPLE:** th.penis, th.penis @Tharwatha")]
        [Remarks("Fun")]
        public async Task PenisAsync(SocketUser? target = null)
        {
            Random random = new();
            var length = random.Next(1, 40);
            var lengthInch = length / 2.54;


            if (target == null || target == Context.User)
            {
                await ReplyAsync($"{Context.User.Mention}, your dick is {length} cm ({lengthInch} inches)");
            }
            else if (target.Id == 966367996408905768)
            {
                await ReplyAsync($"{target.Mention}'s dick is too big to measure, it's over 9000 cm!");
            }
            else
            {
                await ReplyAsync($"{target.Mention}'s dick is {length} cm ({lengthInch} inches)");
            }
        }
    }
}
