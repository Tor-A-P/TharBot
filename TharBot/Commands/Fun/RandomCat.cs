using QuickType;
using Discord.Commands;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class RandomCat : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public RandomCat(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Command("Cat")]
        [Alias("ψατ", "katt")]
        [Summary("Makes the bot send a random cat image.\n" +
                "**USAGE:** th.cat")]
        [Remarks("Fun")]
        public async Task RandomCatAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync("https://api.thecatapi.com/v1/images/search");
                var cat = Cat.FromJson(response);

                await ReplyAsync(cat[0].Url.ToString());
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Cat", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Cat", null, ex);
            }
        }
    }
}
