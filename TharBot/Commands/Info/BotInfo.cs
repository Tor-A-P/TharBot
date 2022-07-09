using Discord.Commands;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class BotInfo : ModuleBase<SocketCommandContext>
    {
        [Command("Botinfo")]
        [Summary("Returns information about this bot.\n" +
                "**USAGE:** th.botinfo")]
        [Remarks("Info")]
        public async Task BotInfoAsync()
        {
            var bot = Context.Client;

            var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder("Info about TharBot");

            var embed = embedBuilder.AddField("Author", "Tharwatha#5189")
                .AddField("Active Guilds", bot.Guilds.Count)
                .AddField("Created at", bot.CurrentUser.CreatedAt.LocalDateTime)
                .AddField("Ping", bot.Latency)
                .WithThumbnailUrl(bot.CurrentUser.GetAvatarUrl(Discord.ImageFormat.Auto, 2048) ?? bot.CurrentUser.GetDefaultAvatarUrl())
                .Build();
                

            await ReplyAsync(embed: embed);
        }
    }
}
