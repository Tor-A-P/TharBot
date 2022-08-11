using Discord.Commands;
using Discord.WebSocket;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Bite : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public Bite(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Command("Bite")]
        [Summary("Bites someone.\n" +
            "**USAGE:** th.bite [USER_MENTION]")]
        [Remarks("Anime Reactions")]
        public async Task BiteAsync(SocketUser? target = null)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync("https://api.otakugifs.xyz/gif?reaction=bite");
                var gif = AnimeReaction.FromJson(response);

                if (target == null || target == Context.User)
                {
                    await ReplyAsync($"{Context.User.Mention} bites a random person!");
                    await ReplyAsync(gif.Url.ToString());
                }
                else
                {
                    await ReplyAsync($"{target.Mention}, {Context.User.Mention} just bit you!");
                    await ReplyAsync(gif.Url.ToString());
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Bite", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Bite", null, ex);
            }
        }
    }
}
