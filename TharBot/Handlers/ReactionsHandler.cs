﻿using Discord;
using Discord.Addons.Hosting;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TharBot.DBModels;

namespace TharBot.Handlers
{
    public class ReactionsHandler : DiscordClientService
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;
        private readonly Random random = new();

        public ReactionsHandler(DiscordSocketClient client, IConfiguration configuration, ILogger<DiscordClientService> logger)
            : base(client, logger)
        {
            _client = client;
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", _configuration);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.ReactionAdded += PollHandling;
            _client.ReactionAdded += FightAttackHandling;
            _client.ReactionAdded += FightSpellHandling;
            _client.ReactionAdded += AttributesHandling;
        }

        private async Task PollHandling(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            try
            {
                if (reaction.UserId == _client.CurrentUser.Id) return;

                Emoji[] pollEmojis =
                {
                    new Emoji("1️⃣"),
                    new Emoji("2️⃣"),
                    new Emoji("3️⃣"),
                    new Emoji("4️⃣"),
                    new Emoji("5️⃣"),
                    new Emoji("6️⃣"),
                    new Emoji("7️⃣"),
                    new Emoji("8️⃣"),
                    new Emoji("9️⃣"),
                    new Emoji("🔟"),
                    new Emoji("😀"),
                    new Emoji("🙂"),
                    new Emoji("😐"),
                    new Emoji("☹"),
                    new Emoji("😢"),
                    new Emoji("😡")
                };
                if (!pollEmojis.Contains(reaction.Emote)) return;

                var recs = db.LoadRecords<Poll>("ActivePolls");

                var activePoll = db.LoadRecordById<Poll>("ActivePolls", message.Id);

                if (activePoll == null) return;
                else
                {
                    var emoji = reaction.Emote;

                    if (!message.HasValue)
                    {
                        var chan = await Client.GetChannelAsync(channel.Id) as IMessageChannel;
                        var msg = await chan.GetMessageAsync(message.Id) as IUserMessage;
                        await msg.RemoveReactionAsync(reaction.Emote, reaction.UserId);
                    }
                    else await message.Value.RemoveReactionAsync(reaction.Emote, reaction.UserId);

                    if (!activePoll.Emojis.Contains(emoji.Name)) return;

                    if (activePoll.Responses == null)
                    {
                        activePoll.Responses.Add(new ActivePollResponse
                        {
                            VoterId = reaction.UserId,
                            Vote = emoji.Name
                        });

                        if (emoji.Name == "😢" || emoji.Name == "😡")
                        {
                            var forGuildId = await Client.GetChannelAsync(channel.Id) as SocketGuildChannel;
                            var resultsChannelSettings = db.LoadRecordById<PulseCheckResultsChannel>("PulsecheckResultsChannel", forGuildId.Guild.Id);
                            var responseChan = await Client.GetChannelAsync(resultsChannelSettings.ResultsChannel) as IMessageChannel;
                            await responseChan.SendMessageAsync($"@Here {reaction.User.Value.Mention} just answered {emoji.Name} to the pulsecheck, maybe someone should check up on them?");
                        }
                    }
                    else
                    {
                        var response = activePoll.Responses.FirstOrDefault(x => x.VoterId == reaction.UserId);
                        if (response != null) return;
                        else
                        {
                            activePoll.Responses.Add(new ActivePollResponse
                            {
                                VoterId = reaction.UserId,
                                Vote = emoji.Name
                            });

                            if (emoji.Name == "😢" || emoji.Name == "😡")
                            {
                                var forGuildId = await Client.GetChannelAsync(channel.Id) as SocketGuildChannel;
                                var resultsChannelSettings = db.LoadRecordById<PulseCheckResultsChannel>("PulsecheckResultsChannel", forGuildId.Guild.Id);
                                var responseChan = await Client.GetChannelAsync(resultsChannelSettings.ResultsChannel) as IMessageChannel;
                                await responseChan.SendMessageAsync($"@Here {reaction.User.Value.Mention} just answered {emoji.Name} to the pulsecheck, maybe someone should check up on them?");
                            }
                        }
                    }

                    db.UpsertRecord("ActivePolls", activePoll.MessageId, activePoll);
                }
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("Bot", null, ex);
            }
        }

        private async Task FightAttackHandling(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.UserId == _client.CurrentUser.Id) return;

            var AttackEmoji = EmoteHandler.Attack;
            if (AttackEmoji.Name != reaction.Emote.Name) return;

            try
            {
                var fight = db.LoadRecordById<GameFight>("ActiveFights", reaction.MessageId);
                var chan = await Client.GetChannelAsync(channel.Id) as IMessageChannel;
                IUserMessage? msg = message.HasValue ? message.Value : await chan.GetMessageAsync(message.Id) as IUserMessage;

                if (fight != null)
                {
                    if (reaction.UserId != fight.UserId) return;
                    var prefix = "";
                    var existingPrefix = db.LoadRecordById<Prefixes>("Prefixes", fight.ServerId);
                    if (existingPrefix != null)
                    {
                        prefix = existingPrefix.Prefix;
                    }
                    else
                    {
                        prefix = _configuration["Prefix"];
                    }
                    var coinReward = fight.Enemy.Level * 50;
                    var expReward = fight.Enemy.Level * 20;
                    await msg.RemoveReactionAsync(reaction.Emote, reaction.UserId);
                    var serverProfile = db.LoadRecordById<GameServerProfile>("GameProfiles", fight.ServerId);
                    var userProfile = serverProfile.Users.Where(x => x.UserId == fight.UserId).FirstOrDefault();
                    var user = Client.GetUser(fight.UserId);
                    fight.TurnNumber++;

                    var userCrit = false;
                    var monsterCrit = false;
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

                    if (fight.Enemy.Debuffs.StunDuration <= 0)
                    {
                        userProfile.CurrentHP -= monsterDamage;

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

                        if (monsterCrit == true) turnText += $"{EmoteHandler.Crit}Critical hit! {fight.Enemy.Name} deals {monsterDamage} damage to {user.Username}!{EmoteHandler.Crit}\n";
                        else turnText += $"{EmoteHandler.Attack}{fight.Enemy.Name} deals {monsterDamage} damage to {user.Username}!{EmoteHandler.Attack}\n";
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
                    }
                    else turnText += $"{fight.Enemy.Name} is stunned for {fight.Enemy.Debuffs.StunDuration} turn(s), and cannot take action!\n";

                    fight.Turns.Add(turnText);
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

            var SpellEmoji = EmoteHandler.Spells;
            if (SpellEmoji.Name != reaction.Emote.Name) return;


        }
        private async Task AttributesHandling(Cacheable<IUserMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            if (reaction.UserId == _client.CurrentUser.Id) return;

            var attributesEmojis = new Emote[]
            {
                EmoteHandler.Strength,
                EmoteHandler.Intelligence,
                EmoteHandler.Dexterity,
                EmoteHandler.Constitution,
                EmoteHandler.Wisdom,
                EmoteHandler.Luck
            };
            if (!attributesEmojis.Contains(reaction.Emote)) return;

            try
            {
                var attributeDialog = db.LoadRecordById<GameAttributeDialog>("ActiveAttributeDialogs", reaction.MessageId);
                var chan = await Client.GetChannelAsync(channel.Id) as IMessageChannel;
                IUserMessage? msg = message.HasValue ? message.Value : await chan.GetMessageAsync(message.Id) as IUserMessage;

                if (attributeDialog != null)
                {
                    if (reaction.UserId != attributeDialog.UserId) return;
                    var user = await Client.GetUserAsync(attributeDialog.UserId) as SocketUser;
                    var serverProfile = db.LoadRecordById<GameServerProfile>("GameProfiles", attributeDialog.ServerId);
                    var userProfile = serverProfile.Users.Where(x => x.UserId == attributeDialog.UserId).FirstOrDefault();
                    var attributeAddedText = "";

                    if (userProfile.AvailableAttributePoints <= 0)
                    {
                        attributeAddedText = "You have no available attribute points to spend!";
                    }
                    else
                    {
                        if (reaction.Emote.Name == EmoteHandler.Strength.Name)
                        {
                            userProfile.Attributes.Strength++;
                            attributeAddedText = $"{EmoteHandler.Strength}You increased your Strength by 1!{EmoteHandler.Strength}";
                        }
                        else if (reaction.Emote.Name == EmoteHandler.Intelligence.Name)
                        {
                            userProfile.Attributes.Intelligence++;
                            attributeAddedText = $"{EmoteHandler.Intelligence}You increased your Intelligence by 1!{EmoteHandler.Intelligence}";
                        }
                        else if (reaction.Emote.Name == EmoteHandler.Dexterity.Name)
                        {
                            userProfile.Attributes.Dexterity++;
                            attributeAddedText = $"{EmoteHandler.Dexterity}You increased your Dexterity by 1!{EmoteHandler.Dexterity}";
                        }
                        else if (reaction.Emote.Name == EmoteHandler.Constitution.Name)
                        {
                            userProfile.Attributes.Constitution++;
                            attributeAddedText = $"{EmoteHandler.Constitution}You increased your Constitution by 1!{EmoteHandler.Constitution}";
                        }
                        else if (reaction.Emote.Name == EmoteHandler.Wisdom.Name)
                        {
                            userProfile.Attributes.Wisdom++;
                            attributeAddedText = $"{EmoteHandler.Wisdom}You increased your Wisdom by 1!{EmoteHandler.Wisdom}";
                        }
                        else if (reaction.Emote.Name == EmoteHandler.Luck.Name)
                        {
                            userProfile.Attributes.Luck++;
                            attributeAddedText = $"{EmoteHandler.Luck}You increased your Luck by 1!{EmoteHandler.Luck}";
                        }
                    }
                    db.UpsertRecord("GameProfiles", attributeDialog.ServerId, serverProfile);
                    await msg.RemoveReactionAsync(reaction.Emote, reaction.UserId);
                    var showAttributesEmbed = await EmbedHandler.CreateAttributeEmbedBuilder(userProfile, user);
                    showAttributesEmbed.AddField("­", attributeAddedText);
                    await msg.ModifyAsync(x => x.Embed = showAttributesEmbed.Build());
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
    }
}
