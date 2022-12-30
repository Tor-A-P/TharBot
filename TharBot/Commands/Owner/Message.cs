using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;

namespace TharBot.Commands.Owner
{
    public class Message : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;

        public Message(DiscordSocketClient client)
        {
            _client = client;
        }

        [Command("Message")]
        [Summary("Makes the bot send a specified message in the specified channel in the specified guild, if it has access to it." +
            "**USAGE:** th.message [CHANNEL_ID] [MESSAGE]")]
        [Remarks("Bot Owner")]
        [RequireOwner]
        public async Task MessageAsync(ulong channelId, [Remainder] string message)
        {
            //var guild = _client.GetGuild(guildId);
            //var channel = guild.GetChannel(channelId) as IMessageChannel;
            var channel = _client.GetChannel(channelId) as IMessageChannel;
            if (channel == null) return;

            try
            {
                await channel.SendMessageAsync(message);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Message", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Message", null, ex);
            }
        }
    }
}
