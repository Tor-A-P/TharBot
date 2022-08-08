using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TharBot.DBModels;

namespace TharBot.Handlers
{
    public class FightHandler : DiscordClientService
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;
        private readonly Random random = new();

        public FightHandler(DiscordSocketClient client, IConfiguration configuration, ILogger<DiscordClientService> logger)
            : base(client, logger)
        {
            _client = client;
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", _configuration);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.ReactionAdded += FightAttackHandling;
            _client.ReactionAdded += FightSpellHandling;
        }

        private async Task FightAttackHandling(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.UserId == _client.CurrentUser.Id) return;

            if (EmoteHandler.Attack.Name != reaction.Emote.Name) return;

            try
            {
                var fight = db.LoadRecordById<GameFight>("ActiveFights", reaction.MessageId);
                var chan = await Client.GetChannelAsync(channel.Id) as IMessageChannel;
                IUserMessage? msg = message.HasValue ? message.Value : await chan.GetMessageAsync(message.Id) as IUserMessage;

                if (fight != null)
                {
                    if (reaction.UserId != fight.UserId) return;
                    var prefix = "";
                    var serverSettings = db.LoadRecordById<ServerSpecifics>("ServerSpecifics", fight.ServerId);
                    if (serverSettings.Prefix != null) prefix = serverSettings.Prefix;
                    else prefix = _configuration["Prefix"];

                    var coinReward = fight.Enemy.Level * 50;
                    var expReward = fight.Enemy.Level * 20;
                    await msg.RemoveReactionAsync(reaction.Emote, reaction.UserId);
                    var serverProfile = db.LoadRecordById<GameServerProfile>("GameProfiles", fight.ServerId);
                    var userProfile = serverProfile.Users.Where(x => x.UserId == fight.UserId).FirstOrDefault();
                    var user = Client.GetUser(fight.UserId);
                    fight.TurnNumber++;

                    var userCrit = false;
                    var userDamage = userProfile.BaseAtk;
                    var userMinDmg = userDamage - (userDamage / 20);
                    var userMaxDmg = userDamage + (userDamage / 20);
                    userDamage = random.Next((int)userMinDmg, (int)userMaxDmg + 1);
                    if (random.Next(1, 101) <= userProfile.CritChance)
                    {
                        userDamage *= 1 + (userProfile.CritDamage / 100);
                        userCrit = true;
                    }
                    userDamage = Math.Floor(userDamage - fight.Enemy.BaseDef);
                    if (userDamage < 0) userDamage = 0;

                    var turnText = "";

                    if (userProfile.Debuffs.StunDuration <= 0)
                    {
                        fight.Enemy.CurrentHP -= userDamage;

                        if (fight.Enemy.Debuffs.HoTDuration >= 1)
                        {
                            turnText += $"{EmoteHandler.HoT}{fight.Enemy.Name} heals {fight.Enemy.Debuffs.HoTStrength} damage from their healing over time effect!{EmoteHandler.HoT}\n";
                            fight.Enemy.CurrentHP += fight.Enemy.Debuffs.HoTStrength;
                        }
                        if (fight.Enemy.Debuffs.DoTDuration >= 1)
                        {
                            turnText += $"{EmoteHandler.DoT}{fight.Enemy.Name} takes {fight.Enemy.Debuffs.DoTStrength} damage from their damage over time effect!{EmoteHandler.DoT}\n";
                            fight.Enemy.CurrentHP -= fight.Enemy.Debuffs.DoTStrength;
                        }

                        if (userCrit == true) turnText += $"{EmoteHandler.Crit} Critical hit! {user.Username} deals {userDamage} damage to {fight.Enemy.Name}!{EmoteHandler.Crit}\n";
                        else turnText += $"{EmoteHandler.Attack}{user.Username} deals {userDamage} damage to {fight.Enemy.Name}!{EmoteHandler.Attack}\n";
                        if (fight.Enemy.CurrentHP <= 0)
                        {
                            turnText += $"{fight.Enemy.Name} is dead! {user.Username} wins the fight!\n" +
                                        $"You receive {EmoteHandler.Coin}{coinReward} TharCoins and {EmoteHandler.Exp}{expReward} EXP!";
                            fight.Turns.Add(turnText);
                            fight.Enemy.CurrentHP = 0;
                            var winnerEmbed = await EmbedHandler.CreateGameEmbed(fight, userProfile, user.Username);
                            await msg.ModifyAsync(x => x.Embed = winnerEmbed);
                            userProfile.NumFightsWon++;
                            userProfile.TharCoins += coinReward;
                            userProfile.Exp += expReward;
                            if (userProfile.Exp >= userProfile.ExpToLevel)
                            {
                                userProfile.Exp -= userProfile.ExpToLevel;
                                userProfile.Level += 1;
                                var levelUpEmbed = await EmbedHandler.CreateBasicEmbedBuilder($"{EmoteHandler.Level}Level up!");
                                levelUpEmbed = levelUpEmbed.WithDescription($"Congratulations {user.Username}, you've reached level {userProfile.Level}!\n" +
                                                                            $"Your health and mana has been refilled.\n" +
                                                                            $"You have gained {GameUserProfile.AttributePointsPerLevel} attribute points, use the {prefix}attributes command to spend them!")
                                                           .WithThumbnailUrl(user.GetAvatarUrl(ImageFormat.Auto, 2048) ?? user.GetDefaultAvatarUrl());
                                userProfile.CurrentHP = userProfile.BaseHP;
                                userProfile.CurrentMP = userProfile.BaseMP;
                                userProfile.AttributePoints += GameUserProfile.AttributePointsPerLevel;

                                await chan.SendMessageAsync(embed: levelUpEmbed.Build());
                            }
                            await msg.RemoveAllReactionsAsync();
                            userProfile.FightInProgress = false;
                            userProfile.Debuffs = new GameDebuffs
                            {
                                StunDuration = 0,
                                HoTDuration = 0,
                                HoTStrength = 0,
                                DoTDuration = 0,
                                DoTStrength = 0
                            };
                            db.DeleteRecord<GameFight>("ActiveFights", reaction.MessageId);
                            db.UpsertRecord("GameProfiles", fight.ServerId, serverProfile);
                            return;
                        }
                    }
                    else turnText += $"{user.Username} is stunned for {userProfile.Debuffs.StunDuration} turn(s), and cannot take action!\n";

                    var enemyTurn = await EnemyTurn(fight, turnText, userProfile, user);
                    fight = enemyTurn.fight;
                    userProfile = enemyTurn.userProfile;

                    if (userProfile.CurrentHP <= 0)
                    {
                        turnText += $"{user.Username} has passed out! {fight.Enemy.Name} wins the fight!";
                        fight.Turns.Add(turnText);
                        userProfile.CurrentHP = 0;
                        var loserEmbed = await EmbedHandler.CreateGameEmbed(fight, userProfile, user.Username);
                        await msg.ModifyAsync(x => x.Embed = loserEmbed);
                        await msg.RemoveAllReactionsAsync();
                        userProfile.FightInProgress = false;
                        userProfile.Debuffs = new GameDebuffs
                        {
                            StunDuration = 0,
                            HoTDuration = 0,
                            HoTStrength = 0,
                            DoTDuration = 0,
                            DoTStrength = 0
                        };
                        db.DeleteRecord<GameFight>("ActiveFights", reaction.MessageId);
                        db.UpsertRecord("GameProfiles", fight.ServerId, serverProfile);
                        return;
                    }

                    var embed = await EmbedHandler.CreateGameEmbed(fight, userProfile, user.Username);
                    await msg.ModifyAsync(x => x.Embed = embed);
                    userProfile.Debuffs.StunDuration--;
                    userProfile.Debuffs.DoTDuration--;
                    userProfile.Debuffs.HoTDuration--;
                    fight.Enemy.Debuffs.StunDuration--;
                    fight.Enemy.Debuffs.DoTDuration--;
                    fight.Enemy.Debuffs.HoTDuration--;
                    db.UpsertRecord("ActiveFights", reaction.MessageId, fight);
                    db.UpsertRecord("GameProfiles", fight.ServerId, serverProfile);
                }
                else
                {
                    await msg.RemoveReactionAsync(reaction.Emote, reaction.UserId);
                }
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("Bot", null, ex);
            }
        }

        private async Task FightSpellHandling(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.UserId == _client.CurrentUser.Id) return;

            if (EmoteHandler.Spells.Name != reaction.Emote.Name) return;


        }
        
        private async Task<(GameFight fight, GameUserProfile userProfile)> EnemyTurn(GameFight fight, string turnText, GameUserProfile userProfile, SocketUser user)
        {
            var monsterCrit = false;
            var monsterDamage = fight.Enemy.BaseAtk;
            var monsterMinDmg = monsterDamage - (monsterDamage / 20);
            var monsterMaxDmg = monsterDamage + (monsterDamage / 20);
            monsterDamage = random.Next((int)monsterMinDmg, (int)monsterMaxDmg + 1);
            if (random.Next(1, 101) <= fight.Enemy.CritChance)
            {
                monsterDamage *= 1 + (fight.Enemy.CritDamage / 100);
                monsterCrit = true;
            }
            monsterDamage = Math.Floor(monsterDamage - userProfile.BaseDef);
            if (monsterDamage < 0) monsterDamage = 0;

            if (userProfile.Debuffs.HoTDuration >= 1)
            {
                turnText += $"{EmoteHandler.HoT}{user.Username} heals {userProfile.Debuffs.HoTStrength} damage from their healing over time effect!{EmoteHandler.HoT}\n";
                userProfile.CurrentHP += userProfile.Debuffs.HoTStrength;
            }
            if (userProfile.Debuffs.DoTDuration >= 1)
            {
                turnText += $"{EmoteHandler.DoT}{user.Username} takes {userProfile.Debuffs.DoTStrength} damage from their damage over time effect!{EmoteHandler.DoT}\n";
                userProfile.CurrentHP -= userProfile.Debuffs.DoTStrength;
            }

            if (fight.Enemy.Debuffs.StunDuration <= 0)
            {
                userProfile.CurrentHP -= monsterDamage;

                if (monsterCrit == true) turnText += $"{EmoteHandler.Crit}Critical hit! {fight.Enemy.Name} deals {monsterDamage} damage to {user.Username}!{EmoteHandler.Crit}\n";
                else turnText += $"{EmoteHandler.Attack}{fight.Enemy.Name} deals {monsterDamage} damage to {user.Username}!{EmoteHandler.Attack}\n";
            }
            else turnText += $"{fight.Enemy.Name} is stunned for {fight.Enemy.Debuffs.StunDuration} turn(s), and cannot take action!\n";

            fight.Turns.Add(turnText);

            return (fight, userProfile);
        }
    }
}
