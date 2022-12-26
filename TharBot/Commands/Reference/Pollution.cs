using Discord.Commands;
using Microsoft.Extensions.Configuration;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Pollution : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _weatherAPI;

        public Pollution(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = config;
            _weatherAPI = _configuration["WeatherAPI"];
        }

        [Command("Pollution")]
        [Alias("AQI", "AirQuality", "AQ")]
        [Summary("Shows pollution data about the location provided.\n" +
                "**USAGE:** th.pollution [LOCATION]\n" +
                "**EXAMPLE:** th.pollution New York")]
        [Remarks("Reference")]
        public async Task PollutionAsync([Remainder] string location)
        {
            try
            {
                var weatherAPIKey = _weatherAPI;
                var httpClient = _httpClientFactory.CreateClient();
                var responseGeo = await httpClient.GetStringAsync($"http://api.openweathermap.org/geo/1.0/direct?q={location}&limit=1&appid={weatherAPIKey}");
                var geocode = Geocode.FromJson(responseGeo);

                try
                {
                    var exTest = geocode[0].Lat;
                }
                catch (IndexOutOfRangeException)
                {
                    var noResultEmbed = await EmbedHandler.CreateErrorEmbed("Pollution", $"Could not find a place called \"{location}\"!");
                    await ReplyAsync(embed: noResultEmbed);
                    return;
                }

                var responsePollution = await httpClient.GetStringAsync(
                   $"https://api.openweathermap.org/data/2.5/air_pollution?lat={geocode[0].Lat}&lon={geocode[0].Lon}&units=metric&appid={weatherAPIKey}");
                var pollution = PollutionResult.FromJson(responsePollution);
                string airQuality = "";

                switch (pollution.List[0].Main.Aqi)
                {
                    case 1:
                        airQuality = "Good";
                        break;
                    case 2:
                        airQuality = "Fair";
                        break;
                    case 3:
                        airQuality = "Moderate";
                        break;
                    case 4:
                        airQuality = "Poor";
                        break;
                    case 5:
                        airQuality = "Very Poor";
                        break;
                    default: break;
                }

                var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder(
                    $"Pollution data for {geocode[0].Name}, {geocode[0].Country} :flag_{geocode[0].Country.ToLower()}:");

                var embed = embedBuilder.AddField("Air Quality:", airQuality)
                    .AddField("Carbon monoxide (CO)", pollution.List[0].Components["co"].ToString("0.###") + "μg/m3", true)
                    .AddField("Nitrogen monoxide (NO)", pollution.List[0].Components["no"].ToString("0.###") + "μg/m3", true)
                    .AddField("Nitrogen dioxide (NO₂)", pollution.List[0].Components["no2"].ToString("0.###") + "μg/m3", true)
                    .AddField("Ozone (O₃)", pollution.List[0].Components["o3"].ToString("0.###") + "μg/m3", true)
                    .AddField("Sulphur dioxide (SO₂)", pollution.List[0].Components["so2"].ToString("0.###") + "μg/m3", true)
                    .AddField("Ammonia (NH₃)", pollution.List[0].Components["nh3"].ToString("0.###") + "μg/m3", true)
                    .WithCurrentTimestamp()
                    .Build();

                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Pollution", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Pollution", null, ex);
            }
        }
    }
}
