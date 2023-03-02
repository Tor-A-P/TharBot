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

                if (aqiResponse.Contains("\"status\":\"error\""))
                {
                    var errorText = aqiResponse.Substring(aqiResponse.LastIndexOf("\"data\":\""), aqiResponse.Length - aqiResponse.LastIndexOf("\"data\":\""));
                    errorText = errorText.Remove(errorText.Length - 1);
                    errorText = errorText.Remove(0, 7);

                    if (errorText == "\"Unknown station\"")
                    {
                        var responseErrorEmbed = await EmbedHandler.CreateErrorEmbed("Pollution", $"Could not find a station for \"{location}\"");
                        await ReplyAsync(embed: responseErrorEmbed);
                        return;
                    }
                    else
                    {
                        var responseErrorEmbed = await EmbedHandler.CreateErrorEmbed("Pollution", $"The API returned the error: {errorText}");
                        await ReplyAsync(embed: responseErrorEmbed);
                        return;
                    } 
                }

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
