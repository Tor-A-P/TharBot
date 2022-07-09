using Discord.Commands;
using Microsoft.Extensions.Configuration;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Weather : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public Weather(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = config;
        }

        [Command("Weather")]
        [Alias("w")]
        [Summary("Returns information about the weather in a specified location.\n" +
                "**USAGE:** th.weather [LOCATION]\n" +
                "**EXAMPLE:** th.weather New York")]
        [Remarks("Reference")]
        public async Task WeatherAsync([Remainder] string location)
        {
            try
            {
                var weatherAPIKey = _configuration["WeatherAPI"];
                var httpClient = _httpClientFactory.CreateClient();
                var responseGeo = await httpClient.GetStringAsync($"http://api.openweathermap.org/geo/1.0/direct?q={location}&limit=1&appid={weatherAPIKey}");
                var geocode = Geocode.FromJson(responseGeo);

                try
                {
                    var exTest = geocode[0].Lat;
                }
                catch (IndexOutOfRangeException)
                {
                    var noResultEmbed = await EmbedHandler.CreateErrorEmbed("Weather", $"Could not find a place called \"{location}\"!");
                    await ReplyAsync(embed: noResultEmbed);
                    return;
                }

                var responseWeather = await httpClient.GetStringAsync(
                   $"https://api.openweathermap.org/data/2.5/weather?lat={geocode[0].Lat}&lon={geocode[0].Lon}&units=metric&appid={weatherAPIKey}");
                var weather = WeatherResult.FromJson(responseWeather);

                var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder(
                    $"Weather for {weather.Name}, {weather.Sys.Country} :flag_{weather.Sys.Country.ToLower()}:");

                embedBuilder = embedBuilder.AddField("Temperature:", $"{weather.Main.Temp}°C / {(weather.Main.Temp * 1.8) + 32:0.##}°F", true)
                    .AddField("Feels like:", $"{weather.Main.FeelsLike}°C / {(weather.Main.FeelsLike * 1.8) + 32:0.##}°F", true)
                    .AddField("Conditions:", $"{weather.Clouds.All}% Clouds, {weather.Weather[0].Description}\n" +
                    $"Wind Speed: {weather.Wind.Speed} km/h / {weather.Wind.Speed / 1.609344:0.##} mph\n" +
                    $"Barometric pressure: {weather.Main.Pressure}hPa, {weather.Main.Humidity}% humidity", false)
                    .WithThumbnailUrl($"http://openweathermap.org/img/wn/{weather.Weather[0].Icon}@2x.png")
                    .WithCurrentTimestamp();



                await ReplyAsync(embed: embedBuilder.Build());
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Weather", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Weather", null, ex);
            }
        }
    }
}
