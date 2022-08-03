using Discord.Commands;
using Microsoft.Extensions.Configuration;
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
                var existingWL = db.LoadRecordById<Whitelist>("WhitelistedGameChannels", Context.Guild.Id);
                if (existingWL != null)
                {
                    if (existingWL.WLChannelId.Any())
                    {
                        if (!existingWL.WLChannelId.Contains(Context.Channel.Id)) return;
                    }
                }

                var existingBL = db.LoadRecordById<Blacklist>("BlacklistedGameChannels", Context.Guild.Id);
                if (existingBL != null)
                {
                    if (existingBL.BLChannelId.Any())
                    {
                        if (existingBL.BLChannelId.Contains(Context.Channel.Id)) return;
                    }
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
                        if (userProfile.LastRespec + TimeSpan.FromHours(24) > DateTime.UtcNow)
                        {
                            var cooldownTime = userProfile.LastRespec.Subtract(DateTime.UtcNow) + TimeSpan.FromHours(24);
                            var respecOnCDEmbed = await EmbedHandler.CreateUserErrorEmbed("Respec command on cooldown",
                                $"You used the respec command too recently, please wait {cooldownTime.Hours} hours, {cooldownTime.Minutes} minutes and {cooldownTime.Seconds} seconds before doing a respec!");
                            await ReplyAsync(embed: respecOnCDEmbed);
                            return;
                        }

                        var totalRespecPoints = strength + dexterity + intelligence + constitution + wisdom + luck;
                        if (totalRespecPoints > userProfile.AttributePoints)
                        {
                            var tooManyPointsEmbed = await EmbedHandler.CreateUserErrorEmbed("Too many points entered",
                                $"The total of the attribute distribution you entered was {totalRespecPoints}, but you only have {userProfile.AttributePoints} to spend!");
                            await ReplyAsync(embed: tooManyPointsEmbed);
                            return;
                        }

                        userProfile.LastRespec = DateTime.UtcNow;
                        userProfile.Attributes = new GameStats
                        {
                            Strength = strength,
                            Intelligence = intelligence,
                            Dexterity = dexterity,
                            Constitution = constitution,
                            Wisdom = wisdom,
                            Luck = luck
                        };
                        userProfile.CurrentMP += GameUserProfile.WisdomMPBonus * wisdom;
                        if (userProfile.CurrentMP > userProfile.BaseMP) userProfile.CurrentMP = userProfile.BaseMP;
                        userProfile.CurrentHP += GameUserProfile.ConstitutionHPBonus * constitution;
                        if (userProfile.CurrentHP > userProfile.BaseHP) userProfile.CurrentHP = userProfile.BaseHP;

                        db.UpsertRecord("GameProfiles", serverProfile.ServerId, serverProfile);

                        var existingPrefix = db.LoadRecordById<Prefixes>("Prefixes", Context.Guild.Id);
                        string? currentPrefix;

                        if (existingPrefix != null) currentPrefix = existingPrefix.Prefix;
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
