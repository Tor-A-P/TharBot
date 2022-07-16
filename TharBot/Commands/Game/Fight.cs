using Discord;
using Discord.Commands;
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
        [Summary("Summons a monster for you to fight. The monster will be close to your level.\n" +
            "You can only start fights with up to 10 monsters per hour (individual cooldown, starts when you defeat the first monster of that period)" +
            "**USAGE:** th.fight")]
        [Remarks("Game")]
        public async Task FightAsync(string monsterName = "")
        {
            try
            {
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

                        Random random = new();
                        var monsterList = db.LoadRecords<GameMonster>("MonsterList");
                        GameMonster? monster = null;
                        if (monsterName != "") monster = monsterList.Where(x => x.Name.ToLower() == monsterName.ToLower()).FirstOrDefault();
                        if (monster == null) monster = monsterList[random.Next(monsterList.Count)];
                        while (monster.MinLevel > userProfile.Level)
                        {
                            monster = monsterList[random.Next(monsterList.Count)];
                        }
                        monster.Level = random.NextInt64(userProfile.Level - 2, userProfile.Level + 2);
                        if (monster.Level < 1) monster.Level = 1;
                        monster.CurrentHP = monster.BaseHP;
                        monster.CurrentMP = monster.BaseMP;
                        var fightEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"{Context.User.Username} vs Level {monster.Level} {monster.Name}");
                        fightEmbed.AddField($"Lv. {userProfile.Level} {Context.User.Username}", $"{EmoteHandler.HP} HP:  {userProfile.CurrentHP} / {userProfile.BaseHP}\n" +
                                                                   $"{EmoteHandler.MP} MP:  {userProfile.CurrentMP} / {userProfile.BaseMP}\n" +
                                                                   $"{EmoteHandler.Attack} Atk: {userProfile.BaseAtk}\n" +
                                                                   $"{EmoteHandler.Defend} Def: {userProfile.BaseDef}", true)
                                  .AddField($"Lv. {monster.Level} {monster.Name}", $"{EmoteHandler.HP} HP:  {monster.CurrentHP} / {monster.BaseHP}\n" +
                                                          $"{EmoteHandler.MP} MP:  {monster.CurrentMP} / {monster.BaseMP}\n" +
                                                          $"{EmoteHandler.Attack} Atk: {monster.BaseAtk}\n" +
                                                          $"{EmoteHandler.Defend} Def: {monster.BaseDef}", true)
                                  .AddField($"A wild {monster.Name} just appeared!", $"What will {Context.User.Username} do?")
                                  .WithFooter("Click the reactions to do actions like attacking, defending, casting spells, or using consumables");
                        var fight = await ReplyAsync(embed: fightEmbed.Build());
                        var emotes = new Emote[]
                        {
                        EmoteHandler.Attack,
                        EmoteHandler.Defend,
                            //EmoteHandler.Consumable,
                            //EmoteHandler.Spells
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