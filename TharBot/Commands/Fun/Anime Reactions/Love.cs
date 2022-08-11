using Discord.Commands;
using Discord.WebSocket;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Love : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public Love(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Command("Love")]
        [Summary("Loves someone.\n" +
            "**USAGE:** th.love [USER_MENTION]")]
        [Remarks("Anime Reactions")]
        public async Task LoveAsync(SocketUser? target = null)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync("https://api.otakugifs.xyz/gif?reaction=love");
                var gif = AnimeReaction.FromJson(response);

                if (target == null)
                {
                    await ReplyAsync($"Are you lonely...? Here, have some love from me! <3");
                    await ReplyAsync(gif.Url.ToString());
                }
                else if (target == Context.User)
                {
                    await ReplyAsync($"{target.Mention} loves themself! <3");
                    await ReplyAsync(gif.Url.ToString());
                }
                else
                {
                    await ReplyAsync($"{target.Mention}, {Context.User.Mention} loves you! <3");
                    await ReplyAsync(gif.Url.ToString());
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Love", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Love", null, ex);
            }
        }
    }
}
