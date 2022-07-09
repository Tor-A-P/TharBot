using Discord.Commands;
using Microsoft.Extensions.Configuration;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Anime : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public Anime(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = config;
        }

        [Command("Anime")]
        [Alias("a", "mal")]
        [Summary("Searches MyAnimeList for an anime and returns information about it\n" +
                "**USAGE:** th.anime [SEARCH_TERM]\n" +
                "**EXAMPLE:** th.anime Serial Experiments Lain")]
        [Remarks("Reference")]
        public async Task AnimeAsync([Remainder] string search)
        {
            try
            {
                var clientId = _configuration["MAL ID"];
                var client = _httpClientFactory.CreateClient();

                var animeListRequest = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.myanimelist.net/v2/anime?q={search}&limit=4");
                animeListRequest.Headers.TryAddWithoutValidation("X-MAL-CLIENT-ID", clientId);

                var animeListResponse = await client.SendAsync(animeListRequest);
                var animeListResult = await animeListResponse.Content.ReadAsStringAsync();
                var animeList = AnimeListResult.FromJson(animeListResult);

                try
                {
                    var testEx = animeList.Data[0].Node.Id;
                }
                catch (IndexOutOfRangeException)
                {
                    var noResultEmbed = await EmbedHandler.CreateErrorEmbed("Anime", $"Could not find any results for \"{search}\"!");
                    await ReplyAsync(embed: noResultEmbed);
                    return;
                }

                var animeRequest = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.myanimelist.net/v2/anime/{animeList.Data[0].Node.Id}?fields=id,title,main_picture,start_date,end_date,mean,status,genres,num_episodes");
                animeRequest.Headers.TryAddWithoutValidation("X-MAL-CLIENT-ID", clientId);

                var animeResponse = await client.SendAsync(animeRequest);
                var animeResult = await animeResponse.Content.ReadAsStringAsync();
                var anime = AnimeResult.FromJson(animeResult);

                string status;
                if (anime.Status == "finished_airing") status = "Finished airing " + anime.EndDate.ToShortDateString();
                else if (anime.Status == "not_yet_aired") status = "Not started airing, starts " + anime.StartDate.ToShortDateString();
                else status = "Currently airing, started " + anime.StartDate.ToShortDateString();

                var genres = "";
                foreach (var genre in anime.Genres)
                {
                    genres += genre.Name + ", ";
                }
                genres = genres.Remove(genres.Length - 2, 2);

                var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder(anime.Title);

                var embed = embedBuilder.WithThumbnailUrl(anime.MainPicture.Large)
                    .AddField("Genres", genres)
                    .AddField("Status", status)
                    .AddField("Episodes", anime.NumEpisodes.ToString(), true)
                    .AddField("Score", anime.Mean.ToString("#.00"), true)
                    .WithUrl($"https://myanimelist.net/anime/{anime.Id}/{anime.Title.Replace(' ', '_')}")
                    .Build();


                await ReplyAsync(embed: embed);

                if (animeList.Data.Length > 3) await ReplyAsync($"Similar search results:\n" +
                    $"<https://myanimelist.net/anime/{animeList.Data[1].Node.Id}/{animeList.Data[1].Node.Title.Replace(' ', '_')}>\n" +
                    $"<https://myanimelist.net/anime/{animeList.Data[2].Node.Id}/{animeList.Data[2].Node.Title.Replace(' ', '_')}>\n" +
                    $"<https://myanimelist.net/anime/{animeList.Data[3].Node.Id}/{animeList.Data[3].Node.Title.Replace(' ', '_')}>");

                else if (animeList.Data.Length > 2) await ReplyAsync($"Similar search results:\n" +
                    $"<https://myanimelist.net/anime/{animeList.Data[1].Node.Id}/{animeList.Data[1].Node.Title.Replace(' ', '_')}>\n" +
                    $"<https://myanimelist.net/anime/{animeList.Data[2].Node.Id}/{animeList.Data[2].Node.Title.Replace(' ', '_')}>");

                else if (animeList.Data.Length > 1) await ReplyAsync($"Similar search result:\n" +
                    $"<https://myanimelist.net/anime/{animeList.Data[1].Node.Id}/{animeList.Data[1].Node.Title.Replace(' ', '_')}>");
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Anime", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Anime", null, ex);
            }
            
        }
    }
}
