using Discord.Commands;
using Discord.WebSocket;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Poke : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public Poke(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Command("Poke")]
        [Summary("Pokes someone.\n" +
            "**USAGE:** th.poke [USER_MENTION]")]
        [Remarks("Anime Reactions")]
        public async Task PokeAsync(SocketUser? target = null)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync("https://api.otakugifs.xyz/gif?reaction=poke");
                var gif = AnimeReaction.FromJson(response);

                if (target == null || target == Context.User)
                {
                    await ReplyAsync($"{Context.User.Mention} pokes wildly around, hitting a random person!");
                    await ReplyAsync(gif.Url.ToString());
                }
                else
                {
                    await ReplyAsync($"{target.Mention}, {Context.User.Mention} pokes you!");
                    await ReplyAsync(gif.Url.ToString());
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Poke", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Poke", null, ex);
            }
        }
    }
}
