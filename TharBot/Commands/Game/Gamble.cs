using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Gamble : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public Gamble(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("Gamble")]
        [Alias("slots")]
        [Summary("Lets you gamble the specified number of points, or all of them. If no amount is specified, it will gamble 50 points.\n" +
            "**USAGE:** th.gamble [AMOUNT]\n" +
            "**EXAMPLES:** th.gamble 42, th.gamble all, th.gamble 50%\n" +
            "**PAYOUTS:**\n" +
            "2 of the same symbol: 3x bet.\n" +
            "3 of the same symbol: 200x bet.")]
        [Remarks("Game")]
        public async Task GambleAsync(string amount = "50")
        {
            try
            {
                if (amount[0] == '-')
                {
                    var noNegativeBetsEmbed = await EmbedHandler.CreateUserErrorEmbed("No negative bets!", $"You can't bet TharCoins you don't have!");
                    await ReplyAsync(embed: noNegativeBetsEmbed);
                    return;
                }

                var serverProfile = db.LoadRecordById<GameServerProfile>("GameProfiles", Context.Guild.Id);
                if (serverProfile == null)
                {
                    var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find server profile", "It seems this server has no profile, try sending a message (not a command) and then use this command again!");
                    await ReplyAsync(embed: noServerProfEmbed);
                }
                else
                {
                    var userProfile = serverProfile.Users.Where(x => x.UserId == Context.User.Id).FirstOrDefault();
                    if (userProfile == null)
                    {
                        var noUserProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                        await ReplyAsync(embed: noUserProfEmbed);
                    }
                    else
                    {
                        if (userProfile.GambaInProgress)
                        {
                            var embed = await EmbedHandler.CreateUserErrorEmbed("Gamble already in progress!", "You already have a running gamble, please wait until it finishes before starting a new one!");
                            await ReplyAsync(embed: embed);
                            return;
                        }

                        Emoji[] emojis =
                        {
                        new Emoji("🍇"),
                        new Emoji("🍈"),
                        new Emoji("🍉"),
                        new Emoji("🍊"),
                        new Emoji("🍋"),
                        new Emoji("🍌"),
                        new Emoji("🍍"),
                        new Emoji("🍎"),
                        new Emoji("🍐"),
                        new Emoji("🍑"),
                        new Emoji("🍒"),
                        new Emoji("🍓"),
                        new Emoji("🆒")
                        };
                        var random = new Random();
                        var regex = new Regex(Regex.Escape($"{EmoteHandler.Slots}"));
                        var chance = 33 + userProfile.BonusGambaChance;
                        var user = Context.User;
                        var embedTitle = "★彡 𝚂𝙻𝙾𝚃 𝙼𝙰𝙲𝙷𝙸𝙽𝙴 ★彡";

                        if (amount.ToLower() == "all" || amount.EndsWith('%'))
                        {
                            double slots = userProfile.TharCoins;
                            if (amount.EndsWith('%'))
                            {
                                amount = amount.Remove(amount.Length - 1);
                                if (int.TryParse(amount, out int result))
                                {
                                    if (result > 100)
                                    {
                                        var invalidInputEmbed = await EmbedHandler.CreateUserErrorEmbed("Invalid input!", $"{amount} is not a valid input, use a positive integer, a percentage expression 100% or lower (like \"50%\"), or just the word \"all\"!");
                                        await ReplyAsync(embed: invalidInputEmbed);
                                        return;
                                    }
                                    slots = System.Math.Floor(slots / 100 * result);
                                }
                                else
                                {
                                    var invalidInputEmbed = await EmbedHandler.CreateUserErrorEmbed("Invalid input!", $"{amount} is not a valid input, use a positive integer, a percentage expression 100% or lower (like \"50%\"), or just the word \"all\"!");
                                    await ReplyAsync(embed: invalidInputEmbed);
                                    return;
                                }
                            }
                            userProfile.GambaInProgress = true;
                            db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);

                            var embed = await EmbedHandler.CreateBasicEmbed(embedTitle, 
                                        $"{EmoteHandler.Blank}{EmoteHandler.BoxDownRight}{EmoteHandler.BoxLeftRight}{EmoteHandler.BoxLeftDownRight}{EmoteHandler.BoxLeftRight}{EmoteHandler.BoxLeftDownRight}{EmoteHandler.BoxLeftRight}{EmoteHandler.BoxLeftDown}\n" +
                                        $"{EmoteHandler.Blank}{EmoteHandler.BoxUpDown}{EmoteHandler.Slots}{EmoteHandler.BoxUpDown}{EmoteHandler.Slots}{EmoteHandler.BoxUpDown}{EmoteHandler.Slots}{EmoteHandler.BoxUpDown}\n" +
                                        $"{EmoteHandler.Blank}{EmoteHandler.BoxUpRight}{EmoteHandler.BoxLeftRight1}{EmoteHandler.BoxLeftUpRight}{EmoteHandler.BoxLeftRight2}{EmoteHandler.BoxLeftUpRight}{EmoteHandler.BoxLeftRight3}{EmoteHandler.BoxUpLeft}\n\n" +
                                        $"Only for **{user.Username}** \n You bet: {EmoteHandler.Coin} {slots}");
                            var slotsMsg = await ReplyAsync(embed: embed);
                            await Task.Delay(3000);
                            Emoji[] resultEmojis = new Emoji[3];
                            resultEmojis[0] = emojis[random.Next(0, emojis.Length)];
                            if (random.Next(1, 101) <= chance)
                            {
                                resultEmojis[1] = resultEmojis[0];
                            }
                            else
                            {
                                do
                                {
                                    resultEmojis[1] = emojis[random.Next(0, emojis.Length)];
                                } while (resultEmojis[1].Name == resultEmojis[0].Name);
                            }
                            resultEmojis[2] = emojis[random.Next(0, emojis.Length)];
                            if (resultEmojis[2].Name == resultEmojis[0].Name)
                            {
                                resultEmojis[2] = emojis[random.Next(0, emojis.Length)];
                            }

                            embed = await EmbedHandler.CreateBasicEmbed(embedTitle, regex.Replace(embed.Description, resultEmojis[0].Name, 1));
                            await slotsMsg.ModifyAsync(x => x.Embed = embed);
                            await Task.Delay(1500);
                            embed = await EmbedHandler.CreateBasicEmbed(embedTitle, regex.Replace(embed.Description, resultEmojis[1].Name, 1));
                            await slotsMsg.ModifyAsync(x => x.Embed = embed);
                            await Task.Delay(1500);
                            embed = await EmbedHandler.CreateBasicEmbed(embedTitle, regex.Replace(embed.Description, resultEmojis[2].Name, 1));
                            await slotsMsg.ModifyAsync(x => x.Embed = embed);

                            if (resultEmojis[0].Name == resultEmojis[1].Name && resultEmojis[1].Name == resultEmojis[2].Name)
                            {
                                var winnerEmbed = await EmbedHandler.CreateBasicEmbedBuilder(embedTitle);
                                winnerEmbed.AddField("­", embed.Description)
                                           .AddField("***JACKPOT!!!***", $"You win {(long)slots * 200} TharCoins!")
                                           .WithFooter("Visual design for slot machine shamelessly lifted from the Lawliet discord bot (https://lawlietbot.xyz/)");
                                userProfile.TharCoins += ((long)slots * 199);
                                userProfile.GambaInProgress = false;
                                await slotsMsg.ModifyAsync(x => x.Embed = winnerEmbed.Build());

                                db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);
                            }
                            else if (resultEmojis[0].Name == resultEmojis[1].Name || resultEmojis[1].Name == resultEmojis[2].Name || resultEmojis[0].Name == resultEmojis[2].Name)
                            {
                                var winnerEmbed = await EmbedHandler.CreateBasicEmbedBuilder(embedTitle);
                                winnerEmbed.AddField("­", embed.Description)
                                           .AddField("A WINNER IS YOU!", $"You win {(long)slots * 3} TharCoins!")
                                           .WithFooter("Visual design for slot machine shamelessly lifted from the Lawliet discord bot (https://lawlietbot.xyz/)"); ;
                                userProfile.TharCoins += ((long)slots * 2);
                                userProfile.GambaInProgress = false;
                                await slotsMsg.ModifyAsync(x => x.Embed = winnerEmbed.Build());

                                db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);
                            }
                            else
                            {
                                var loserEmbed = await EmbedHandler.CreateBasicEmbedBuilder(embedTitle);
                                loserEmbed.AddField("­", embed.Description)
                                           .AddField("You lose!", $"You lost {(long)slots} TharCoins :(")
                                           .WithFooter("Visual design for slot machine shamelessly lifted from the Lawliet discord bot (https://lawlietbot.xyz/)"); ;
                                userProfile.TharCoins -= (long)slots;
                                userProfile.GambaInProgress = false;
                                await slotsMsg.ModifyAsync(x => x.Embed = loserEmbed.Build());

                                db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);
                            }
                        }
                        else if (long.TryParse(amount, out var slots))
                        {
                            if (slots > userProfile.TharCoins)
                            {
                                var notEnoughPointsEmbed = await EmbedHandler.CreateUserErrorEmbed("Not enough TharCoin", $"You only have {userProfile.TharCoins} TharCoins, you can't spend {slots} on gambling!");
                                await ReplyAsync(embed: notEnoughPointsEmbed);
                            }
                            else
                            {
                                userProfile.GambaInProgress = true;
                                db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);
                                var embed = await EmbedHandler.CreateBasicEmbed(embedTitle,
                                        $"{EmoteHandler.Blank}{EmoteHandler.BoxDownRight}{EmoteHandler.BoxLeftRight}{EmoteHandler.BoxLeftDownRight}{EmoteHandler.BoxLeftRight}{EmoteHandler.BoxLeftDownRight}{EmoteHandler.BoxLeftRight}{EmoteHandler.BoxLeftDown}\n" +
                                        $"{EmoteHandler.Blank}{EmoteHandler.BoxUpDown}{EmoteHandler.Slots}{EmoteHandler.BoxUpDown}{EmoteHandler.Slots}{EmoteHandler.BoxUpDown}{EmoteHandler.Slots}{EmoteHandler.BoxUpDown}\n" +
                                        $"{EmoteHandler.Blank}{EmoteHandler.BoxUpRight}{EmoteHandler.BoxLeftRight1}{EmoteHandler.BoxLeftUpRight}{EmoteHandler.BoxLeftRight2}{EmoteHandler.BoxLeftUpRight}{EmoteHandler.BoxLeftRight3}{EmoteHandler.BoxUpLeft}\n\n" +
                                        $"Only for **{user.Username}** \n You bet: {EmoteHandler.Coin} {slots}");
                                var slotsMsg = await ReplyAsync(embed: embed);
                                await Task.Delay(3000);
                                Emoji[] resultEmojis = new Emoji[3];
                                resultEmojis[0] = emojis[random.Next(0, emojis.Length)];
                                if (random.Next(1, 101) < chance)
                                {
                                    resultEmojis[1] = resultEmojis[0];
                                }
                                else
                                {
                                    do
                                    {
                                        resultEmojis[1] = emojis[random.Next(0, emojis.Length)];
                                    } while (resultEmojis[1].Name == resultEmojis[0].Name);
                                }
                                resultEmojis[2] = emojis[random.Next(0, emojis.Length)];
                                if (resultEmojis[2].Name == resultEmojis[0].Name)
                                {
                                    resultEmojis[2] = emojis[random.Next(0, emojis.Length)];
                                }

                                embed = await EmbedHandler.CreateBasicEmbed(embedTitle, regex.Replace(embed.Description, resultEmojis[0].Name, 1));
                                await slotsMsg.ModifyAsync(x => x.Embed = embed);
                                await Task.Delay(1500);
                                embed = await EmbedHandler.CreateBasicEmbed(embedTitle, regex.Replace(embed.Description, resultEmojis[1].Name, 1));
                                await slotsMsg.ModifyAsync(x => x.Embed = embed);
                                await Task.Delay(1500);
                                embed = await EmbedHandler.CreateBasicEmbed(embedTitle, regex.Replace(embed.Description, resultEmojis[2].Name, 1));
                                await slotsMsg.ModifyAsync(x => x.Embed = embed);

                                if (resultEmojis[0].Name == resultEmojis[1].Name && resultEmojis[1].Name == resultEmojis[2].Name)
                                {
                                    var winnerEmbed = await EmbedHandler.CreateBasicEmbedBuilder(embedTitle);
                                    winnerEmbed.AddField("­", embed.Description)
                                               .AddField("***JACKPOT!!!***", $"You win {slots * 200} TharCoins!")
                                               .WithFooter("Visual design for slot machine shamelessly lifted from the Lawliet discord bot (https://lawlietbot.xyz/)"); ;
                                    userProfile.TharCoins += (slots * 199);
                                    userProfile.GambaInProgress = false;
                                    await slotsMsg.ModifyAsync(x => x.Embed = winnerEmbed.Build());

                                    db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);
                                }
                                else if (resultEmojis[0].Name == resultEmojis[1].Name || resultEmojis[1].Name == resultEmojis[2].Name || resultEmojis[0].Name == resultEmojis[2].Name)
                                {
                                    var winnerEmbed = await EmbedHandler.CreateBasicEmbedBuilder(embedTitle);
                                    winnerEmbed.AddField("­", embed.Description)
                                               .AddField("A WINNER IS YOU!", $"You win {slots * 3} TharCoins!")
                                               .WithFooter("Visual design for slot machine shamelessly lifted from the Lawliet discord bot (https://lawlietbot.xyz/)"); ;
                                    userProfile.TharCoins += (slots * 2);
                                    userProfile.GambaInProgress = false;
                                    await slotsMsg.ModifyAsync(x => x.Embed = winnerEmbed.Build());

                                    db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);
                                }
                                else
                                {
                                    var loserEmbed = await EmbedHandler.CreateBasicEmbedBuilder(embedTitle);
                                    loserEmbed.AddField("­", embed.Description)
                                               .AddField("You lose!", $"You lost {slots} TharCoins :(")
                                               .WithFooter("Visual design for slot machine shamelessly lifted from the Lawliet discord bot (https://lawlietbot.xyz/)"); ;
                                    userProfile.TharCoins -= slots;
                                    userProfile.GambaInProgress = false;
                                    await slotsMsg.ModifyAsync(x => x.Embed = loserEmbed.Build());

                                    db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);
                                }
                            }
                        }
                        else
                        {
                            var invalidInputEmbed = await EmbedHandler.CreateUserErrorEmbed("Invalid input!", $"{amount} is not a valid input, use a positive integer, a percentage expression 100% or lower (like \"50%\"), or just the word \"all\"!");
                            await ReplyAsync(embed: invalidInputEmbed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Gamble", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Gamble", null, ex);
            }
        }
    }
}
