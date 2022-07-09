using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.Handlers;

namespace TharBot.Commands
{

    public class TestCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;

        public TestCommands(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Command("Ping")]
        [Summary("Simply returns \"Pong!\"\n**USAGE:** th.ping")]
        [Remarks("Test Commands")]
        public async Task PingAsync()
        {
            var bot = Context.Client;
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync($"Pong! {bot.Latency}ms");
        }

        [Command("Embed")]
        [Summary("Testing command for messing with embeds")]
        [Remarks("Test Commands")]
        public async Task EmbedAsync()
        {
            var embed = await EmbedHandler.CreateBasicEmbedBuilder("Test");
            embed = embed.WithImageUrl("https://www.reddit.com/gallery/v9ajjp");
            await ReplyAsync(embed: embed.Build());
        }
    }
}