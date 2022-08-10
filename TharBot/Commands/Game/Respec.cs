using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands.Game
{
    public class Respec : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;
        private readonly IConfiguration _configuration;

        public Respec(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
            _configuration = config;
        }

        [Command("Respec")]
        [Summary("Refunds all your currently spent attribute points so you can redistribute them. Respec has a 24 hour cooldown.\n" +
            "You can also specify the distribution of attributes you want after the respec by entering numbers for each stat separated by spaces." +
            "The order of the attributes is: strength, intelligence, dexterity, constitution, wisdom, luck.\n" +
            "**USAGE:** th.respec, th.respec [ATTRIBUTE_DISTRIBUTION]\n" +
            "**EXAMPLES:** th.respec 25 0 0 10 0 0, th.respec 0 0 30 0 5 0")]
        [Remarks("Game")]
        public async Task RespecAsync(int strength = 0, int intelligence = 0, int dexterity = 0, int constitution = 0, int wisdom = 0, int luck = 0)
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

                var userProfile = await db.LoadRecordByIdAsync<GameUser>("UserProfiles", Context.User.Id);
                if (userProfile == null)
                {
                    var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                    await ReplyAsync(embed: noServerProfEmbed);
                }
                else
                {
                    var serverStats = userProfile.Servers.Where(x => x.ServerId == Context.Guild.Id).FirstOrDefault();
                    if (serverStats == null)
                    {
                        var noUserProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                        await ReplyAsync(embed: noUserProfEmbed);
                    }
                    else
                    {
                        //if (serverStats.LastRespec + TimeSpan.FromHours(24) > DateTime.UtcNow)
                        //{
                        //    var cooldownTime = serverStats.LastRespec.Subtract(DateTime.UtcNow) + TimeSpan.FromHours(24);
                        //    var respecOnCDEmbed = await EmbedHandler.CreateUserErrorEmbed("Respec command on cooldown",
                        //        $"You used the respec command too recently, please wait {cooldownTime.Hours} hours, {cooldownTime.Minutes} minutes and {cooldownTime.Seconds} seconds before doing a respec!");
                        //    await ReplyAsync(embed: respecOnCDEmbed);
                        //    return;
                        //}

                        var totalRespecPoints = strength + dexterity + intelligence + constitution + wisdom + luck;
                        if (totalRespecPoints > serverStats.AttributePoints)
                        {
                            var tooManyPointsEmbed = await EmbedHandler.CreateUserErrorEmbed("Too many points entered",
                                $"The total of the attribute distribution you entered was {totalRespecPoints}, but you only have {serverStats.AttributePoints} to spend!");
                            await ReplyAsync(embed: tooManyPointsEmbed);
                            return;
                        }

                        serverStats.LastRespec = DateTime.UtcNow;
                        serverStats.Attributes = new GameStats
                        {
                            Strength = strength,
                            Intelligence = intelligence,
                            Dexterity = dexterity,
                            Constitution = constitution,
                            Wisdom = wisdom,
                            Luck = luck
                        };
                        serverStats.CurrentMP += GameServerStats.WisdomMPBonus * wisdom;
                        if (serverStats.CurrentMP > serverStats.BaseMP) serverStats.CurrentMP = serverStats.BaseMP;
                        serverStats.CurrentHP += GameServerStats.ConstitutionHPBonus * constitution;
                        if (serverStats.CurrentHP > serverStats.BaseHP) serverStats.CurrentHP = serverStats.BaseHP;

                        var update = Builders<GameUser>.Update.Set(x => x.Servers, userProfile.Servers);
                        await db.UpdateUserAsync<GameUser>("UserProfiles", userProfile.UserId, update);

                        string? currentPrefix;

                        if (serverSettings.Prefix != null) currentPrefix = serverSettings.Prefix;
                        else currentPrefix = _configuration["Prefix"];

                        if (totalRespecPoints == 0)
                        {
                            var embed = await EmbedHandler.CreateBasicEmbed("Attributes respecced", $"Your attributes have been reset to 0, use {currentPrefix}attributes to put them back in your desired attributes!");
                            await ReplyAsync(embed: embed);
                        }
                        else
                        {
                            var embed = await EmbedHandler.CreateBasicEmbed("Attributes respecced", $"Your attributes have been redistributed as specified!");
                            await ReplyAsync(embed: embed);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Respec", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Respec", null, ex);
            }
        }
    }
}
