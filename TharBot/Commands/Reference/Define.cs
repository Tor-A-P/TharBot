using Discord.Commands;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Define : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public Define(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Command("Define")]
        [Alias("d")]
        [Summary("Looks up the dictionary definition of a word.\n" +
                "**USAGE:** th.define [WORD]\n" +
                "**EXAMPLE:** th.define hello")]
        [Remarks("Reference")]
        public async Task DefineAsync([Remainder] string search)
        {
            var httpClient = _httpClientFactory.CreateClient();
            string response;
            
            try
            {
                response = await httpClient.GetStringAsync($"https://api.dictionaryapi.dev/api/v2/entries/en/{search}");
            }
            catch (HttpRequestException)
            {
                var noResultEmbed = await EmbedHandler.CreateErrorEmbed("Define", $"No definition found for \"{search}\"!");
                await ReplyAsync(embed: noResultEmbed);
                return;
            }

            var definition = DefineResult.FromJson(response);
            string? pronunciation = null;

            try
            {
                var exTest = definition[0].Meanings[0].Definitions[0].DefinitionDefinition;
            }
            catch (IndexOutOfRangeException)
            {
                var noResultEmbed = await EmbedHandler.CreateErrorEmbed("Define", $"No definition found for \"{search}\"!");
                await ReplyAsync(embed: noResultEmbed);
                return;
            }

            try
            {
                if (string.IsNullOrEmpty(definition[0].Phonetics[0].Audio))
                {
                    if (string.IsNullOrEmpty(definition[0].Phonetics[0].Text)) pronunciation = "No info";
                    else pronunciation = definition[0].Phonetics[0].Text;
                }
            }
            catch (IndexOutOfRangeException)
            {
                pronunciation = "No info";
            }

            var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder(char.ToUpper(definition[0].Word[0]) + definition[0].Word.Remove(0, 1));

            var embed = embedBuilder.AddField("Definition", definition[0].Meanings[0].Definitions[0].DefinitionDefinition)
                .AddField("Example", definition[0].Meanings[0].Definitions[0].Example ?? "No example found")
                .AddField("Pronunciation", pronunciation ?? definition[0].Phonetics[0].Audio)
                .Build();
                
            await ReplyAsync(embed: embed);
        }
    }
}
