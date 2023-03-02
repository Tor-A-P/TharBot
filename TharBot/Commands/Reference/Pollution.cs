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

        public Pollution(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = config;
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
                var weatherAPIKey = _configuration["WeatherAPI"];
                var httpClient = _httpClientFactory.CreateClient();
                var aqiResponse = await httpClient.GetStringAsync($"https://api.waqi.info/feed/{location}/?token=78d05a09b86fcd2ec6c1cd9519de2c26a5a4da5a");
                var aqi = Aqi.FromJson(aqiResponse);

                await ReplyAsync(aqiResponse);

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
