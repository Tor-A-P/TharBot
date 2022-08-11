using Discord.Commands;
using Discord.WebSocket;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Punch : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public Punch(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Command("Punch")]
        [Summary("Punches someone.\n" +
            "**USAGE:** th.punch [USER_MENTION]")]
        [Remarks("Anime Reactions")]
        public async Task PunchAsync(SocketUser? target = null)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync("https://api.otakugifs.xyz/gif?reaction=punch");
                var gif = AnimeReaction.FromJson(response);

                if (target == null || target == Context.User)
                {
                    await ReplyAsync($"{Context.User} swings wildly around, hitting someone!");
                    await ReplyAsync(gif.Url.ToString());
                }
                else
                {
                    await ReplyAsync($"{target.Mention}, {Context.User.Mention} punches you!");
                    await ReplyAsync(gif.Url.ToString());
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Punch", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Punch", null, ex);
            }
        }
    }
}
