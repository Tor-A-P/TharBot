using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Avatar : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;

        public Avatar(DiscordSocketClient client)
        {
            _client = client;
        }

        [Command("Avatar")]
        [Alias("Pic", "pfp")]
        [Summary("Returns the high-quality version avatar of the user given, by default the local server avatar, optionally the global avatar. If no user specified, returns command user's avatar.\n" +
                "**USAGE:** th.avatar, th.avatar [@USER_MENTION], th.avatar [@USER_MENTION] global")]
        [Remarks("Info")]
        public async Task GetUserAvatarAsync(SocketUser? user = null, string mode = "local")
        {
            try
            {
                if (user == null) user = Context.User;

                var guildUser = Context.Guild.GetUser(user.Id);
                
                if (mode.ToLower() == "local")
                {
                    var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder($"Avatar for {guildUser.Username}#{guildUser.Discriminator}");

                    var embed = embedBuilder.WithImageUrl(guildUser.GetGuildAvatarUrl(Discord.ImageFormat.Auto, 2048) ?? guildUser.GetAvatarUrl(Discord.ImageFormat.Auto, 2048) ?? guildUser.GetDefaultAvatarUrl())
                        .Build();

                    await ReplyAsync(embed: embed);
                }
                else if (mode.ToLower() == "global")
                {
                    var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder($"Global avatar for {guildUser.Username}#{guildUser.Discriminator}");

                    var embed = embedBuilder.WithImageUrl(guildUser.GetAvatarUrl(Discord.ImageFormat.Auto, 2048) ?? guildUser.GetDefaultAvatarUrl())
                        .Build();

                    await ReplyAsync(embed: embed);
                }
                else
                {
                    var wrongModeEmbed = await EmbedHandler.CreateUserErrorEmbed("Avatar", "Please use either \"local\", \"global\" or no modifier!");

                    await ReplyAsync(embed: wrongModeEmbed);
                } 
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Avatar", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Avatar", null, ex);
            }
        }
    }
}
