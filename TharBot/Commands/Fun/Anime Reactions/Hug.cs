using Discord.Commands;
using Discord.WebSocket;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Hug : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public Hug(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Command("Hug")]
        [Summary("Hugs someone.\n" +
            "**USAGE:** th.hug [USER_MENTION]")]
        [Remarks("Anime Reactions")]
        public async Task HugAsync(SocketUser? target = null)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync("https://api.otakugifs.xyz/gif?reaction=hug");
                var gif = AnimeReaction.FromJson(response);

                if (target == null || target == Context.User)
                {
                    await ReplyAsync($"Do you need a hug? Here you go! <3");
                    await ReplyAsync(gif.Url.ToString());
                }
                else
                {
                    await ReplyAsync($"{target.Mention}, {Context.User.Mention} hugs you!");
                    await ReplyAsync(gif.Url.ToString());
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Hug", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Hug", null, ex);
            }
        }
    }
}
