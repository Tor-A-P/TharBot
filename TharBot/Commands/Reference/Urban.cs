using Discord.Commands;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Urban : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public Urban(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Command("Urban")]
        [Alias("u")]
        [Summary("Searches a term on urban dictionary.\n" +
                "**USAGE:** th.urban [TERM]\n" +
                "**EXAMPLE:** th.urban wat")]
        [Remarks("Reference")]
        public async Task UrbanAsync([Remainder] string search)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                var response = await httpClient.GetStringAsync($"https://api.urbandictionary.com/v0/define?term={search}");
                var urban = UrbanResult.FromJson(response);

                try
                {
                    var exTest = urban.List[0].Definition;
                }
                catch (IndexOutOfRangeException)
                {
                    var noResultEmbed = await EmbedHandler.CreateErrorEmbed("Urban", $"Could not find any definition for {search}!");
                    await ReplyAsync(embed: noResultEmbed);
                    return;
                }

                var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder(search);

                var embed = embedBuilder.AddField($"Definition for {search}", urban.List[0].Definition)
                    .AddField("Example:", urban.List[0].Example)
                    .WithFooter($"Definition written by {urban.List[0].Author}")
                    .Build();



                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Urban", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Urban", null, ex);
            }
        }
    }
}
