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
                var pollutionAPIKey = _configuration["PollutionAPI"];
                var httpClient = _httpClientFactory.CreateClient();
                var aqiResponse = await httpClient.GetStringAsync($"https://api.waqi.info/feed/{location}/?token={pollutionAPIKey}");

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
                var aqiString = "";
                var aqiValue = aqi.Data.Aqi;
                if (aqiValue < 50) { aqiString = "Good!"; }
                else if (aqiValue >= 50 && aqiValue < 100) aqiString = "Moderate";
                else if (aqiValue >= 100 && aqiValue < 150) aqiString = "Unhealthy for sensitive groups";
                else if (aqiValue >= 150 && aqiValue < 200) aqiString = "Unhealthy";
                else if (aqiValue >= 200 && aqiValue < 300) aqiString = "Very Unhealthy";
                else aqiString = "Hazardous!";

                var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder($"Pollution values for \"{location}\"");

                embedBuilder = embedBuilder.AddField("Air Quality Index:", $"{aqi.Data.Aqi} - {aqiString}");
                if (aqi.Data.Iaqi.Pm25 != null) embedBuilder.AddField("PM₂.₅", aqi.Data.Iaqi.Pm25.V, true);
                if (aqi.Data.Iaqi.Pm10 != null) embedBuilder.AddField("PM₁₀", aqi.Data.Iaqi.Pm10.V, true);
                if (aqi.Data.Iaqi.O3 != null) embedBuilder.AddField("O₃", aqi.Data.Iaqi.O3.V, true);
                if (aqi.Data.Iaqi.No2 != null) embedBuilder.AddField("NO₂", aqi.Data.Iaqi.No2.V, true);
                if (aqi.Data.Iaqi.So2 != null) embedBuilder.AddField("SO₂", aqi.Data.Iaqi.So2.V, true);
                if (aqi.Data.Iaqi.Co != null) embedBuilder.AddField("CO", aqi.Data.Iaqi.Co.V, true);

                var embed = embedBuilder.WithCurrentTimestamp().Build();

                await ReplyAsync(embed:embed);

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
