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
                var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
                if (serverSettings.GameWLChannelId != null)
                {
                    if (serverSettings.GameWLChannelId.Any())
                    {
                        if (!serverSettings.GameWLChannelId.Contains(Context.Channel.Id)) return;
                    }
                }

                if (serverSettings.GameBLChannelId != null)
                {
                    if (serverSettings.GameBLChannelId.Any())
                    {
                        if (serverSettings.GameBLChannelId.Contains(Context.Channel.Id)) return;
                    }
                }

                var userList = await db.LoadRecordsAsync<GameUser>("UserProfiles");
                var statsList = new List<GameServerStats>();
                foreach (var user in userList)
                {
                    var stats = user.Servers.Where(x => x.ServerId == Context.Guild.Id).FirstOrDefault();
                    if (stats == null) continue;
                    statsList.Add(stats);
                }

                var embed = flag.ToLower() switch
                {
                    "messages" or "msg" or "msgs" => await EmbedCreator("msg", statsList),
                    "exp" or "xp" or "experience" or "level" or "lvl" => await EmbedCreator("exp", statsList),
                    "coins" or "money" or "tharcoins" => await EmbedCreator("coins", statsList),
                    "fights" or "fight" or "combat" or "monsters" => await EmbedCreator("fights", statsList),
                    _ => await EmbedHandler.CreateUserErrorEmbed("Invalid sorting flag", $"\"{flag}\" is not a valid statistic to sort by, " +
                                            $"valid flags include: messages, exp, coins, fights. If you use the command without a flag at all, it will default to messages."),
                };
                if (embed == null)
                {
                    embed = await EmbedHandler.CreateUserErrorEmbed("Invalid sorting flag", $"\"{flag}\" is not a valid statistic to sort by, " +
                                            $"valid flags include: messages, exp, coins, fights. If you use the command without a flag at all, it will default to messages.");
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

        public async Task<Embed?> EmbedCreator(string flag, List<GameServerStats> profiles)
        {
            try
            {
                var embed = await EmbedHandler.CreateBasicEmbedBuilder("Leaderboard for this server");
                var nameFieldString = "";
                var valueFieldString = "";
                var leaderBoardPos = 1;
                var footerString = "";

                if (flag == "msg")
                {
                    var sortedProfiles = profiles.OrderByDescending(x => x.NumMessages);
                    foreach (var profile in sortedProfiles)
                    {
                        var user = await _client.GetUserAsync(profile.UserId);
                        if (leaderBoardPos <= 15)
                        {
                            if (user.Id == Context.User.Id) nameFieldString += $"{leaderBoardPos}. {EmoteHandler.You}{user.Mention}\n";
                            else nameFieldString += $"{leaderBoardPos}. {user.Mention}\n";
                            valueFieldString += $"{profile.NumMessages}\n";
                        }

                        if (user.Id == Context.User.Id) footerString = $"You are at position {leaderBoardPos} with {profile.NumMessages} messages sent.";
                        if (footerString != "" && leaderBoardPos > 15) break;
                        leaderBoardPos++;
                    }
                    embed.AddField("Username", nameFieldString, true)
                         .AddField("Total Messages", valueFieldString, true)
                         .WithFooter(footerString);
                }
                else if (flag == "exp")
                {
                    var sortedProfiles = profiles.OrderByDescending(x => x.TotalExp());
                    foreach (var profile in sortedProfiles)
                    {
                        var user = await _client.GetUserAsync(profile.UserId);
                        if (leaderBoardPos <= 15)
                        {
                            if (user.Id == Context.User.Id) nameFieldString += $"{leaderBoardPos}. {EmoteHandler.You}{user.Mention}\n";
                            else nameFieldString += $"{leaderBoardPos}. {user.Mention}\n";
                            valueFieldString += $"{profile.TotalExp()}\n";
                        }

                        if (user.Id == Context.User.Id) footerString = $"You are at position {leaderBoardPos} with {profile.TotalExp()} total EXP.";
                        if (footerString != "" && leaderBoardPos > 15) break;
                        leaderBoardPos++;
                    }
                    embed.AddField("Username", nameFieldString, true)
                         .AddField("Total EXP", valueFieldString, true)
                         .WithFooter(footerString);
                }
                else if (flag == "coins")
                {
                    var sortedProfiles = profiles.OrderByDescending(x => x.TharCoins);
                    foreach (var profile in sortedProfiles)
                    {
                        var user = await _client.GetUserAsync(profile.UserId);
                        if (leaderBoardPos <= 15)
                        {
                            if (user.Id == Context.User.Id) nameFieldString += $"{leaderBoardPos}. {EmoteHandler.You}{user.Mention}\n";
                            else nameFieldString += $"{leaderBoardPos}. {user.Mention}\n";
                            valueFieldString += $"{profile.TharCoins}\n";
                        }

                        if (user.Id == Context.User.Id) footerString = $"You are at position {leaderBoardPos} with {profile.TharCoins} TharCoins.";
                        if (footerString != "" && leaderBoardPos > 15) break;
                        leaderBoardPos++;
                    }
                    embed.AddField("Username", nameFieldString, true)
                         .AddField("TharCoins", valueFieldString, true)
                         .WithFooter(footerString);
                }
                else if (flag == "fights")
                {
                    var sortedProfiles = profiles.OrderByDescending(x => x.NumFightsWon);
                    foreach (var profile in sortedProfiles)
                    {
                        var user = await _client.GetUserAsync(profile.UserId);
                        if (leaderBoardPos <= 15)
                        {
                            if (user.Id == Context.User.Id) nameFieldString += $"{leaderBoardPos}. {EmoteHandler.You}{user.Mention}\n";
                            else nameFieldString += $"{leaderBoardPos}. {user.Mention}\n";
                            valueFieldString += $"{profile.NumFightsWon}\n";
                        }

                        if (user.Id == Context.User.Id) footerString = $"You are at position {leaderBoardPos} with {profile.NumFightsWon} fights won.";
                        if (footerString != "" && leaderBoardPos > 15) break;
                        leaderBoardPos++;
                    }
                    embed.AddField("Username", nameFieldString, true)
                         .AddField("Fights won", valueFieldString, true)
                         .WithFooter(footerString);
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
