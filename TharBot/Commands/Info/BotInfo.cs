using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class BotInfo : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;

        public BotInfo(DiscordSocketClient client)
        {
            _client = client;
        }

        [Command("Botinfo")]
        [Summary("Returns information about this bot.\n" +
                "**USAGE:** th.botinfo")]
        [Remarks("Info")]
        public async Task BotInfoAsync()
        {
            var botClient = Context.Client;
            var owner = _client.GetUser(212161497256689665);
            var guild = _client.GetGuild(Context.Guild.Id);
            var botUser = guild.GetUser(Context.Client.CurrentUser.Id);
            

            var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder("Info about TharBot");

            var embed = embedBuilder.AddField("Author", $"{owner.Mention}", true)
                .AddField("Active Guilds", botClient.Guilds.Count, true)
                .AddField("Created at", TimestampTag.FromDateTimeOffset(botClient.CurrentUser.CreatedAt))
                .AddField("Joined at", TimestampTag.FromDateTimeOffset((DateTimeOffset)botUser.JoinedAt))
                .AddField("Source code", "https://github.com/Tor-A-P/TharBot")
                .WithThumbnailUrl(botClient.CurrentUser.GetAvatarUrl(Discord.ImageFormat.Auto, 2048) ?? botClient.CurrentUser.GetDefaultAvatarUrl())
                .Build();
                

            await ReplyAsync(embed: embed);
        }
    }
}
