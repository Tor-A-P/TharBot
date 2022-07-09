using Discord.Commands;
using Microsoft.Extensions.Configuration;
using QuickType;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Imgur : ModuleBase<SocketCommandContext>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public Imgur(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = config;
        }

        [Command("Imgur")]
        [Alias("upload")]
        [Summary("Uploads an image to imgur via link or attachment. Will prioritize attachments over links.\n" +
                "**USAGE:** th.imgur [IMAGE_URL], th.imgur [IMAGE_ATTACHED]\n" +
                "**EXAMPLE:** th.imgur ")]
        [Remarks("Utility")]
        public async Task ImgurAsync([Remainder] string? image = null)
        {
            if (image == null && Context.Message.Attachments.Count == 0)
            {
                var noImageEmbed = await EmbedHandler.CreateUserErrorEmbed("Imgur", "Please provide either an image url or an image");
                await ReplyAsync(embed: noImageEmbed);
                return;
            }
            if (image == null && Context.Message.Attachments.Count == 1)
            {
                image = Context.Message.Attachments.FirstOrDefault().Url;
            }
            if (!(image.ToLower().Contains(".jpg") ||
                image.ToLower().Contains(".gif") ||
                image.ToLower().Contains(".apng") ||
                image.ToLower().Contains(".tiff") ||
                image.ToLower().Contains(".mp4") ||
                image.ToLower().Contains(".mpeg") ||
                image.ToLower().Contains(".avi") ||
                image.ToLower().Contains(".webm") ||
                image.ToLower().Contains(".quicktime") ||
                image.ToLower().Contains(".png")))
            {
                var wrongFormatEmbed = await EmbedHandler.CreateUserErrorEmbed("Imgur", "File format not supported!");
                await ReplyAsync(embed: wrongFormatEmbed);
                return;
            }
            try
            {
                var clientId = _configuration["Imgur ClientID"];
                var client = _httpClientFactory.CreateClient();

                var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.imgur.com/3/upload");
                request.Headers.TryAddWithoutValidation("Authorization", $"Client-ID {{{{{clientId}}}}}");

                var multipartContent = new MultipartFormDataContent
                {
                    { new StringContent(image), "image" },
                    { new StringContent("url"), "type" }
                };
                request.Content = multipartContent;

                var response = await client.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();

                var imgur = ImgurResult.FromJson(result);

                if (!response.IsSuccessStatusCode)
                {
                    var noSuccessEmbed = await EmbedHandler.CreateErrorEmbed("Imgur", $"{response.StatusCode} - {response.ReasonPhrase}");
                    await ReplyAsync(embed: noSuccessEmbed);
                }
                else
                {
                    var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder("Image uploaded!");
                    embedBuilder = embedBuilder.WithImageUrl(imgur.Data.Link.ToString())
                                   .WithDescription("Image link: " + imgur.Data.Link);
                    await ReplyAsync(embed: embedBuilder.Build());
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Imgur", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Imgur", null, ex);
            }
        }
    }
}
