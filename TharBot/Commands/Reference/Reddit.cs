using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Reddit;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Reddit : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;

        public Reddit(IConfiguration config)
        {
            _configuration = config;
        }

        [Command("Reddit")]
        [Summary("Returns a random/top post from a specified subreddit. If no [FLAG] specified, returns random post.\n" +
            "Doesn't work with \"reserved\" subs, like r/all and r/popular, also doesn't embed images if the post has multiple attachments" +
            "(so they're in a reddit gallery), but at least it works!" +
            "**USAGE:** th.reddit [SUBREDDIT] [FLAG]\n" +
            "**EXAMPLE:** th.reddit /r/techsupportgore random, th.reddit techsupportgore top")]
        [Remarks("Reference")]
        public async Task RedditAsync(string subreddit, string flag = "")
        {
            try
            {
                if (subreddit.Length >= 3)
                {
                    if (subreddit.ToLower().Substring(0, 3) == "/r/")
                    {
                        subreddit = subreddit.Remove(0, 3);
                    }
                }
                
                var reddit = new RedditClient(appId: _configuration["RedditAppId"], appSecret: _configuration["RedditAppSecret"], refreshToken: _configuration["RedditRefreshToken"]);

                var subR = reddit.Subreddit(subreddit).About();

                var channel = Context.Channel as SocketTextChannel;

                if ((bool)subR.Over18)
                {
                    if (!channel.IsNsfw)
                    {
                        await ReplyAsync($"{subreddit} is marked as NSFW, but this channel is not. Try using it in an NSFW-marked channel instead!");
                        return;
                    }
                }

                var random = new Random();
                var post = subR.Posts.New[random.Next(subR.Posts.New.Count)];

                if (flag.ToLower() == "top")
                {
                    post = subR.Posts.Top[0];
                }
                else if (flag != "" && flag.ToLower() != "random")
                {
                    var wrongFlagEmbed = await EmbedHandler.CreateUserErrorEmbed("Reddit", "Wrong string used for [FLAG], please use either \"random\", \"top\" or nothing");
                    await ReplyAsync(embed: wrongFlagEmbed);
                    return;
                }

                var reply = $"{post.Title}\n" +
                            $"{post.Listing.URL}\n" +
                            $"<http://www.reddit.com{post.Permalink}>\n" +
                            $"Posted by {post.Author}";
                await ReplyAsync(reply);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Reddit", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Reddit", null, ex);
            }
            
        }
    }
}
