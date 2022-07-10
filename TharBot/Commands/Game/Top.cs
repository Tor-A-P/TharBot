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
        [Summary("Shows the top posters in the server, sorted by a choosable statistic, by default number of messages the bot has seen from users.\n" +
            "**USAGE:** th.top, th.top [SORTING_FLAG]\n" +
            "**EXAMPLES:** th.top, th.top coins, th.top exp, th.top fights")]
        [Remarks("Game")]
        public async Task TopAsync(string flag = "messages")
        {
            try
            {
                var serverProfile = db.LoadRecordById<GameServerProfile>("GameProfiles", Context.Guild.Id);
                if (serverProfile == null || serverProfile.Users.Where(x => x.UserId == Context.User.Id) == null)
                {
                    var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find server profile", "It seems this server has no profile, try sending a message (not a command) and then use this command again!");
                    await ReplyAsync(embed: noServerProfEmbed);
                    return;
                }

                var embed = flag.ToLower() switch
                {
                    "messages" or "msg" or "msgs" => await EmbedCreator("msg", serverProfile),
                    "exp" or "xp" or "experience" or "level" or "lvl" => await EmbedCreator("exp", serverProfile),
                    "coins" or "money" or "tharcoins" => await EmbedCreator("coins", serverProfile),
                    "fights" or "fight" or "combat" or "monsters" => await EmbedCreator("fights", serverProfile),
                    _ => await EmbedHandler.CreateUserErrorEmbed("Invalid sorting flag", $"\"{flag}\" is not a valid statistic to sort by, " +
                                            $"valid flags include: messages, exp, coins. If you use the command without a flag at all, it will default to messages."),
                };
                if (embed == null)
                {
                    embed = await EmbedHandler.CreateUserErrorEmbed("Invalid sorting flag", $"\"{flag}\" is not a valid statistic to sort by, " +
                                            $"valid flags include: messages, exp, coins. If you use the command without a flag at all, it will default to messages.");
                }

                await ReplyAsync(embed: embed);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Leaderboard", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Top", null, ex);
            }
        }

        public async Task<Embed?> EmbedCreator(string flag, GameServerProfile profile)
        {
            try
            {
                var embed = await EmbedHandler.CreateBasicEmbedBuilder("Leaderboard for this server");
                var nameFieldString = "";
                var valueFieldString = "";
                var leaderBoardPos = 1;
                if (flag == "msg")
                {
                    var sortedUserProfiles = profile.Users.OrderByDescending(x => x.NumMessages);
                    foreach (var userProfile in sortedUserProfiles)
                    {
                        var user = await _client.GetUserAsync(userProfile.UserId);
                        nameFieldString += $"{leaderBoardPos}. {user.Mention}\n";
                        valueFieldString += $"{userProfile.NumMessages}\n";
                        leaderBoardPos++;
                    }
                    embed.AddField("Username", nameFieldString, true)
                         .AddField("Total Messages", valueFieldString, true);
                }
                else if (flag == "coins")
                {
                    var sortedUserProfiles = profile.Users.OrderByDescending(x => x.TharCoins);
                    foreach (var userProfile in sortedUserProfiles)
                    {
                        var user = await _client.GetUserAsync(userProfile.UserId);
                        nameFieldString += $"{leaderBoardPos}. {user.Mention}\n";
                        valueFieldString += $"{userProfile.TharCoins}\n";
                        leaderBoardPos++;
                    }
                    embed.AddField("Username", nameFieldString, true)
                         .AddField("Total TharCoins", valueFieldString, true);
                }
                else if (flag == "fights")
                {
                    var sortedUserProfiles = profile.Users.OrderByDescending(x => x.NumFightsWon);
                    foreach (var userProfile in sortedUserProfiles)
                    {
                        var user = await _client.GetUserAsync(userProfile.UserId);
                        nameFieldString += $"{leaderBoardPos}. {user.Mention}\n";
                        valueFieldString += $"{userProfile.NumFightsWon}\n";
                        leaderBoardPos++;
                    }
                    embed.AddField("Username", nameFieldString, true)
                         .AddField("Total Fights Won", valueFieldString, true);
                }
                else
                {
                    var sortedUserProfiles = profile.Users.OrderByDescending(x => x.TotalExp());
                    foreach (var userProfile in sortedUserProfiles)
                    {
                        var user = await _client.GetUserAsync(userProfile.UserId);
                        nameFieldString += $"{leaderBoardPos}. {user.Mention}\n";
                        valueFieldString += $"{userProfile.TotalExp()}\n";
                        leaderBoardPos++;
                    }
                    embed.AddField("Username", nameFieldString, true)
                         .AddField("Total EXP", valueFieldString, true);
                }

                return embed.Build();
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Leaderboard", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Top", null, ex);
                return null;
            }
        }
    }
}
