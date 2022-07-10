using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Profile : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public Profile(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("Profile")]
        [Summary("Shows your or someone else's profile for this server, displaying exp, money, and stats\n" +
            "**USAGE:** th.profile, th.profile [@USER_MENTION]\n" +
            "**EXAMPLES:** th.profile Tharwatha, th.profile @Tharwatha#5189")]
        [Remarks("Game")]
        public async Task ProfileAsync(SocketUser? user = null)
        {
            try
            {
                if (user == null) user = Context.User;
                var serverProfile = db.LoadRecordById<GameServerProfile>("GameProfiles", Context.Guild.Id);
                if (serverProfile == null)
                {
                    var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find server profile", "It seems this server has no profile, try sending a message (not a command) and then use this command again!");
                    await ReplyAsync(embed: noServerProfEmbed);
                    return;
                }
                var userProfile = serverProfile.Users.Where(x => x.UserId == user.Id).FirstOrDefault();
                if (userProfile == null)
                {
                    var noUserProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                    await ReplyAsync(embed: noUserProfEmbed);
                    return;
                }

                var embed = await EmbedHandler.CreateBasicEmbedBuilder($"Profile for {user}");
                embed.AddField($"{EmoteHandler.Level} Level: {userProfile.Level}",
                                   $"{EmoteHandler.HP} HP: {userProfile.CurrentHP:N0} / {userProfile.BaseHP:N0}\n" +
                                   $"{EmoteHandler.Strength} Strength: {userProfile.Attributes.Strength}\n" +
                                   $"{EmoteHandler.Intelligence} Intelligence: {userProfile.Attributes.Intelligence}\n" +
                                   $"{EmoteHandler.Dexterity} Dexterity: {userProfile.Attributes.Dexterity}\n" +
                                   $"{EmoteHandler.Crit} Crit Chance: {userProfile.CritChance}%", true)
                     .AddField($"{EmoteHandler.Exp} Exp: {userProfile.Exp:N0} / {userProfile.ExpToLevel:N0}\n",
                                   $"{EmoteHandler.MP} MP: {userProfile.CurrentMP:N0} / {userProfile.BaseMP:N0}\n" +
                                   $"{EmoteHandler.Constitution} Constitution: {userProfile.Attributes.Constitution}\n" +
                                   $"{EmoteHandler.Wisdom} Wisdom: {userProfile.Attributes.Wisdom}\n" +
                                   $"{EmoteHandler.Luck} Luck: {userProfile.Attributes.Luck}\n" +
                                   $"{EmoteHandler.Crit} Crit Damage: {userProfile.CritDamage:N0}%", true)
                     .AddField("­", $"{EmoteHandler.Coin} TharCoins: {userProfile.TharCoins:N0}")
                     .WithThumbnailUrl(Context.User.GetAvatarUrl(ImageFormat.Auto, 2048) ?? Context.User.GetDefaultAvatarUrl());
                await ReplyAsync(embed: embed.Build());
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Profile", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Profile", null, ex);
            }
        }
    }
}
