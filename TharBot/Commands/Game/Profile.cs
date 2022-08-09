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

                if (user == null) user = Context.User;
                var userProfile = await db.LoadRecordByIdAsync<GameUser>("UserProfiles", user.Id);
                if (userProfile == null)
                {
                    if (user == Context.User)
                    {
                        var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                        await ReplyAsync(embed: noServerProfEmbed);
                        return;
                    }
                    else
                    {
                        userProfile = new GameUser
                        {
                            UserId = user.Id,
                            Servers = new List<GameServerStats>(),
                            Revision = 0
                        };
                    }
                    
                }
                var serverStats = userProfile.Servers.Where(x => x.ServerId == Context.Guild.Id).FirstOrDefault();
                if (serverStats == null)
                {
                    if (user == Context.User)
                    {
                        var noUserProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                        await ReplyAsync(embed: noUserProfEmbed);
                        return;
                    }
                    else
                    {
                        Random random = new();
                        serverStats = new GameServerStats
                        {
                            ServerId = Context.Guild.Id,
                            NextRewards = DateTime.UtcNow + TimeSpan.FromMinutes(1),
                            TharCoins = 10,
                            Exp = random.Next(8, 13),
                            Level = 1,
                            Attributes = new GameStats
                            {
                                Strength = 0,
                                Dexterity = 0,
                                Intelligence = 0,
                                Constitution = 0,
                                Wisdom = 0,
                                Luck = 0
                            },
                            AttributePoints = GameServerStats.StartingAttributePoints,
                            NumMessages = 1,
                            Debuffs = new GameDebuffs
                            {
                                StunDuration = 0,
                                HoTDuration = 0,
                                HoTStrength = 0,
                                DoTDuration = 0,
                                DoTStrength = 0
                            }
                        };
                        serverStats.CurrentHP = serverStats.BaseHP;
                        serverStats.CurrentMP = serverStats.BaseMP;
                        userProfile.Servers.Add(serverStats);
                        await db.InsertRecordAsync("UserProfiles", userProfile);
                    }
                }

                var embed = await EmbedHandler.CreateBasicEmbedBuilder($"Profile for {user}");
                embed.AddField($"{EmoteHandler.Level} Level: {serverStats.Level}",
                                   $"{EmoteHandler.HP} HP: {serverStats.CurrentHP:N0} / {serverStats.BaseHP:N0}\n" +
                                   $"{EmoteHandler.Attack} Atk: {serverStats.BaseAtk}\n" +
                                   $"{EmoteHandler.Strength} Strength: {serverStats.Attributes.Strength}\n" +
                                   $"{EmoteHandler.Intelligence} Intelligence: {serverStats.Attributes.Intelligence}\n" +
                                   $"{EmoteHandler.Dexterity} Dexterity: {serverStats.Attributes.Dexterity}\n" +
                                   $"{EmoteHandler.Crit} Crit Chance: {serverStats.CritChance}%\n" +
                                   $"{EmoteHandler.Spells} Spellpower: {serverStats.SpellPower}", true)
                     .AddField($"{EmoteHandler.Exp} Exp: {serverStats.Exp:N0} / {serverStats.ExpToLevel:N0}\n",
                                   $"{EmoteHandler.MP} MP: {serverStats.CurrentMP:N0} / {serverStats.BaseMP:N0}\n" +
                                   $"{EmoteHandler.Defense} Def: {serverStats.BaseDef}\n" +
                                   $"{EmoteHandler.Constitution} Constitution: {serverStats.Attributes.Constitution}\n" +
                                   $"{EmoteHandler.Wisdom} Wisdom: {serverStats.Attributes.Wisdom}\n" +
                                   $"{EmoteHandler.Luck} Luck: {serverStats.Attributes.Luck}\n" +
                                   $"{EmoteHandler.Crit} Crit Damage: {serverStats.CritDamage:N0}%", true)
                     .AddField("­", $"{EmoteHandler.Coin} TharCoins: {serverStats.TharCoins:N0}")
                     .WithThumbnailUrl(user.GetAvatarUrl(ImageFormat.Auto, 2048) ?? user.GetDefaultAvatarUrl());
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
