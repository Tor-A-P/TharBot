using Discord.Commands;
using Discord.WebSocket;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Lick : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public Lick(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Command("Lick")]
        [Summary("Licks someone.\n" +
            "**USAGE:** th.lick [USER_MENTION]")]
        [Remarks("Anime Reactions")]
        public async Task LickAsync(SocketUser? target = null)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync("https://api.otakugifs.xyz/gif?reaction=lick");
                var gif = AnimeReaction.FromJson(response);

                if (target == null || target == Context.User)
                {
                    await ReplyAsync($"Are you trying to lick the air? Here, lick this!");
                    await ReplyAsync(gif.Url.ToString());
                }
                else
                {
                    await ReplyAsync($"{target.Mention}, {Context.User.Mention} licks you!");
                    await ReplyAsync(gif.Url.ToString());
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Lick", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Lick", null, ex);
            }
        }
    }
}
