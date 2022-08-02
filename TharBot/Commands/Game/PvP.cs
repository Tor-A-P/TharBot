using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class PvP : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public PvP(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("PvP")]
        [Summary("Starts a fight with another user. The opposing user will have full health for the fight, but will only use regular attacks (for now).\n" +
                 "**USAGE:** th.pvp [USER_MENTION]\n" +
                 "**EXAMPLE:** th.pvp @Tharwatha#5189")]
        [Remarks("Game")]
        public async Task FightAsync(SocketUser? enemy = null)
        {
            try
            {
                if (enemy == null)
                {
                    var noMentionEmbed = await EmbedHandler.CreateUserErrorEmbed("No user mentioned", "Please mention a user to fight in this command");
                    await ReplyAsync(embed: noMentionEmbed);
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
                        if (userProfile.FightPeriodStart + TimeSpan.FromHours(1) <= DateTime.UtcNow)
                        {
                            userProfile.FightsThisHour = 0;
                            userProfile.FightPeriodStart = DateTime.UtcNow;
                        }

                        if (userProfile.FightsThisHour >= 10)
                        {
                            var cooldownTime = userProfile.FightPeriodStart.Subtract(DateTime.UtcNow) + TimeSpan.FromHours(1);
                            var embed = await EmbedHandler.CreateUserErrorEmbed("Too many fights!",
                                $"You have already started 10 fights in the past hour, please wait {cooldownTime.Minutes} minute(s) and {cooldownTime.Seconds} second(s) before starting another!");
                            await ReplyAsync(embed: embed);
                            return;
                        }

                        if (userProfile.FightInProgress)
                        {
                            var embed = await EmbedHandler.CreateUserErrorEmbed("Fight already in progress", "You are already fighting another monster, finish that fight first! (Or wait 5 minutes for the fight to time out)");
                            await ReplyAsync(embed: embed);
                            return;
                        }

                        var enemyProfile = serverProfile.Users.Where(x => x.UserId == enemy.Id).FirstOrDefault();
                        if (enemyProfile == null)
                        {
                            Random random = new();

                            enemyProfile = new GameUserProfile
                            {
                                UserId = enemy.Id,
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
                                AttributePoints = GameUserProfile.StartingAttributePoints,
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
                            enemyProfile.CurrentHP = enemyProfile.BaseHP;
                            enemyProfile.CurrentMP = enemyProfile.BaseMP;
                            serverProfile.Users.Add(enemyProfile);
                        }
                        if (enemyProfile.Debuffs == null)
                        {
                            enemyProfile.Debuffs = new GameDebuffs
                            {
                                StunDuration = 0,
                                HoTDuration = 0,
                                HoTStrength = 0,
                                DoTDuration = 0,
                                DoTStrength = 0
                            };
                        }
                        var monster = new GameMonster
                        {
                            Name = enemy.Username,
                            Level = enemyProfile.Level,
                            Stats = enemyProfile.Attributes,
                            MinLevel = 1,
                            CurrentHP = enemyProfile.BaseHP,
                            CurrentMP = enemyProfile.BaseMP,
                            Debuffs = enemyProfile.Debuffs
                        };
                        var fightEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"{Context.User.Username} vs Level {monster.Level} {monster.Name}");
                        fightEmbed.AddField($"Lv. {userProfile.Level} {Context.User.Username}", $"{EmoteHandler.HP} HP:  {userProfile.CurrentHP} / {userProfile.BaseHP}\n" +
                                                                   $"{EmoteHandler.MP} MP:  {userProfile.CurrentMP} / {userProfile.BaseMP}\n" +
                                                                   $"{EmoteHandler.Attack} Atk: {userProfile.BaseAtk}\n" +
                                                                   $"{EmoteHandler.Defense} Def: {userProfile.BaseDef}", true)
                                  .AddField($"Lv. {monster.Level} {monster.Name}", $"{EmoteHandler.HP} HP:  {monster.CurrentHP} / {monster.BaseHP}\n" +
                                                          $"{EmoteHandler.MP} MP:  {monster.CurrentMP} / {monster.BaseMP}\n" +
                                                          $"{EmoteHandler.Attack} Atk: {monster.BaseAtk}\n" +
                                                          $"{EmoteHandler.Defense} Def: {monster.BaseDef}", true)
                                  .AddField($"A wild {monster.Name} just appeared!", $"What will {Context.User.Username} do?")
                                  .WithFooter("Click the reactions to do actions like attacking, defending, casting spells, or using consumables");
                        var fight = await ReplyAsync(embed: fightEmbed.Build());
                        var emotes = new Emote[]
                        {
                        EmoteHandler.Attack,
                        //EmoteHandler.Spells,
                        //EmoteHandler.Cunsumables
                        };
                        var gameFight = new GameFight
                        {
                            MessageId = fight.Id,
                            TurnNumber = 0,
                            LastMoveTime = DateTime.UtcNow,
                            Enemy = monster,
                            ServerId = Context.Guild.Id,
                            UserId = userProfile.UserId,
                            ChannelId = Context.Channel.Id,
                            Turns = new List<string>()
                        };
                        db.InsertRecord("ActiveFights", gameFight);
                        userProfile.FightInProgress = true;
                        userProfile.FightsThisHour++;
                        db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);
                        await fight.AddReactionsAsync(emotes);
                    }
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Fight", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Fight", null, ex);
            }
        }
    }
}