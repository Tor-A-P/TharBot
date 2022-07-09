using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;


namespace TharBot.Commands
{
    public class Top : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;
        private readonly DiscordSocketClient _client;

        public Top(IConfiguration config, DiscordSocketClient client)
        {
            db = new MongoCRUDHandler("TharBot", config);
            _client = client;
        }

        [Command("Top")]
        [Alias("Leaderboard")]
        [Summary("Shows the top posters in the server by EXP")]
        [Remarks("Game")]
        public async Task TopAsync()
        {
            var ServerProfiles = db.LoadRecords<GameServerProfile>("GameProfiles");
            if (ServerProfiles == null)
            {
                var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find server profile", "It seems this server has no profile, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noServerProfEmbed);
                return;
            }
            var embed = await EmbedHandler.CreateBasicEmbedBuilder("Leaderboard for this server");
            var nameFieldString = "";
            var expFieldString = "";
            foreach (var profile in ServerProfiles)
            {
                if (profile.ServerId == Context.Guild.Id)
                {
                    var sortedProfiles = profile.Users.OrderByDescending(x => x.TotalExp());
                    var leaderBoardPos = 1;
                    foreach (var userProfile in sortedProfiles)
                    {
                        var user = await _client.GetUserAsync(userProfile.UserId);
                        nameFieldString += $"{leaderBoardPos}. {user.Mention}\n";
                        expFieldString += $"{userProfile.TotalExp()}\n";
                        leaderBoardPos++;
                    }
                }
            }
            embed.AddField("Username", nameFieldString, true)
                 .AddField("Total EXP", expFieldString, true);
            await ReplyAsync(embed: embed.Build());
        }
    }
}
