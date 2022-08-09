using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Fight : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public Fight(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("Fight")]
        [Summary("Summons a monster for you to fight, or fights another player. The monster will be close to your level.\n" +
            "You can only start fights with up to 10 enemies per hour (individual cooldown, starts when you defeat the first monster of that period)\n" +
            "**USAGE:** th.fight, th.fight [USER_MENTION]\n" +
            "**EXAMPLES:** th.fight, th.fight @Tharwatha#5189, th.fight tharwatha")]
        [Remarks("Game")]
        public async Task FightAsync(SocketUser? enemy = null)
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
                        if (serverStats.FightPeriodStart + TimeSpan.FromHours(1) <= DateTime.UtcNow)
                        {
                            serverStats.FightsThisHour = 0;
                            serverStats.FightPeriodStart = DateTime.UtcNow;
                        }

                        if (serverStats.FightsThisHour >= 10)
                        {
                            var cooldownTime = serverStats.FightPeriodStart.Subtract(DateTime.UtcNow) + TimeSpan.FromHours(1);
                            var embed = await EmbedHandler.CreateUserErrorEmbed("Too many fights!",
                                $"You have already started 10 fights in the past hour, please wait {cooldownTime.Minutes} minute(s) and {cooldownTime.Seconds} second(s) before starting another!");
                            await ReplyAsync(embed: embed);
                            return;
                        }

                        if (serverStats.FightInProgress)
                        {
                            var embed = await EmbedHandler.CreateUserErrorEmbed("Fight already in progress", "You are already fighting another monster, finish that fight first! (Or wait 5 minutes for the fight to time out)");
                            await ReplyAsync(embed: embed);
                            return;
                        }

                        Random random = new();
                        var monsterList = await db.LoadRecordsAsync<GameMonster>("MonsterList");
                        GameMonster? monster = null;
                        if (enemy != null)
                        {
                            var enemyProfile = await db.LoadRecordByIdAsync<GameUser>("UserProfiles", enemy.Id);
                            if (enemyProfile == null)
                            {
                                enemyProfile = new GameUser
                                {
                                    UserId = enemy.Id,
                                    Servers = new List<GameServerStats>()
                                };
                            }
                            if (enemyProfile.Servers == null) enemyProfile.Servers = new List<GameServerStats>();

                            var enemyStats = enemyProfile.Servers.Where(x => x.ServerId == Context.Guild.Id).FirstOrDefault();
                            if (enemyStats == null)
                            {
                                enemyStats = new GameServerStats
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
                                enemyStats.CurrentHP = enemyStats.BaseHP;
                                enemyStats.CurrentMP = enemyStats.BaseMP;
                                enemyProfile.Servers.Add(enemyStats);
                            }
                            if (enemyStats.Debuffs == null)
                            {
                                enemyStats.Debuffs = new GameDebuffs
                                {
                                    StunDuration = 0,
                                    HoTDuration = 0,
                                    HoTStrength = 0,
                                    DoTDuration = 0,
                                    DoTStrength = 0
                                };
                            }
                            
                            monster = new GameMonster
                            {
                                Name = enemy.Username,
                                Level = enemyStats.Level,
                                Stats = enemyStats.Attributes,
                                MinLevel = 1,
                                CurrentHP = enemyStats.BaseHP,
                                CurrentMP = enemyStats.BaseMP,
                                Debuffs = enemyStats.Debuffs
                            };
                        }

                        if (monster == null)
                        {
                            monster = monsterList[random.Next(monsterList.Count)];
                            while (monster.MinLevel > serverStats.Level)
                            {
                                monster = monsterList[random.Next(monsterList.Count)];
                            }
                            monster.Level = random.NextInt64(serverStats.Level - 2, serverStats.Level + 2);
                            if (monster.Level < 1) monster.Level = 1;
                        }
                        monster.CurrentHP = monster.BaseHP;
                        monster.CurrentMP = monster.BaseMP;
                        monster.Debuffs = new GameDebuffs
                        {
                            StunDuration = 0,
                            HoTDuration = 0,
                            HoTStrength = 0,
                            DoTDuration = 0,
                            DoTStrength = 0
                        };
                        var fightEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"{Context.User.Username} vs Level {monster.Level} {monster.Name}");
                        fightEmbed.AddField($"Lv. {serverStats.Level} {Context.User.Username}", $"{EmoteHandler.HP} HP:  {serverStats.CurrentHP} / {serverStats.BaseHP}\n" +
                                                                   $"{EmoteHandler.MP} MP:  {serverStats.CurrentMP} / {serverStats.BaseMP}\n" +
                                                                   $"{EmoteHandler.Attack} Atk: {serverStats.BaseAtk}\n" +
                                                                   $"{EmoteHandler.Defense} Def: {serverStats.BaseDef}\n" +
                                                                   $"{EmoteHandler.Spells}Spellpower: {serverStats.SpellPower}", true)
                                  .AddField($"Lv. {monster.Level} {monster.Name}", $"{EmoteHandler.HP} HP:  {monster.CurrentHP} / {monster.BaseHP}\n" +
                                                          $"{EmoteHandler.MP} MP:  {monster.CurrentMP} / {monster.BaseMP}\n" +
                                                          $"{EmoteHandler.Attack} Atk: {monster.BaseAtk}\n" +
                                                          $"{EmoteHandler.Defense} Def: {monster.BaseDef}\n" +
                                                          $"{EmoteHandler.Spells}Spellpower: {monster.SpellPower}", true)
                                  .AddField($"A wild {monster.Name} just appeared!", $"What will {Context.User.Username} do?")
                                  .WithFooter("Click the reactions to do actions like attacking, casting spells, or using consumables (only attack is implemented so far)");
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
                            ServerId = serverStats.ServerId,
                            UserId = userProfile.UserId,
                            ChannelId = Context.Channel.Id,
                            Turns = new List<string>()
                        };
                        await db.InsertRecordAsync("ActiveFights", gameFight);
                        serverStats.FightInProgress = true;
                        serverStats.FightsThisHour++;
                        await db.UpsertRecordAsync("UserProfiles", Context.User.Id, userProfile);
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