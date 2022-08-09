using Discord;
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

                var forGuildId = await Client.GetChannelAsync(reaction.Channel.Id) as SocketGuildChannel;
                var serverSpecifics = db.LoadRecordById<ServerSpecifics>("ServerSpecifics", forGuildId.Guild.Id);

                if (serverSpecifics.Polls == null) return;

                var activePoll = serverSpecifics.Polls.Where(x => x.MessageId == reaction.MessageId).FirstOrDefault();

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
                            var resultsChannelSettings = db.LoadRecordById<ServerSpecifics>("ServerSpecifics", forGuildId.Guild.Id).PCResultsChannel;
                            if (resultsChannelSettings != null)
                            {
                                var responseChan = await Client.GetChannelAsync((ulong)resultsChannelSettings) as IMessageChannel;
                                await responseChan.SendMessageAsync($"{reaction.User.Value.Mention} just answered {emoji.Name} to the pulsecheck, maybe someone should check up on them?");
                            }
                            else
                            {
                                var responseChan = await Client.GetChannelAsync(activePoll.ChannelId) as IMessageChannel;
                                await responseChan.SendMessageAsync($"{reaction.User.Value.Mention} just answered {emoji.Name} to the pulsecheck, maybe someone should check up on them?");
                            }
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
                                var resultsChannelSettings = db.LoadRecordById<ServerSpecifics>("ServerSpecifics", forGuildId.Guild.Id).PCResultsChannel;
                                if (resultsChannelSettings != null)
                                {
                                    var responseChan = await Client.GetChannelAsync((ulong)resultsChannelSettings) as IMessageChannel;
                                    await responseChan.SendMessageAsync($"{reaction.User.Value.Mention} just answered {emoji.Name} to the pulsecheck, maybe someone should check up on them?");
                                }
                                else
                                {
                                    var responseChan = await Client.GetChannelAsync(activePoll.ChannelId) as IMessageChannel;
                                    await responseChan.SendMessageAsync($"{reaction.User.Value.Mention} just answered {emoji.Name} to the pulsecheck, maybe someone should check up on them?");
                                }
                            }
                        }
                    }

                    db.UpsertRecord("ServerSpecifics", serverSpecifics.ServerId, serverSpecifics);
                }
            }
            catch (Exception ex)
            {
                await LoggingHandler.LogCriticalAsync("Bot", null, ex);
            }
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
                var forGuildId = await Client.GetChannelAsync(channel.Id) as SocketGuildChannel;
                var chan = await Client.GetChannelAsync(channel.Id) as IMessageChannel;
                IUserMessage? msg = message.HasValue ? message.Value : await chan.GetMessageAsync(message.Id) as IUserMessage;

                var serverSpecifics = db.LoadRecordById<ServerSpecifics>("ServerSpecifics", forGuildId.Guild.Id);
                if (serverSpecifics.AttributeDialogs == null) return;
                var attributeDialog = serverSpecifics.AttributeDialogs.Where(x => x.MessageId == message.Id).FirstOrDefault();
                

                if (attributeDialog != null)
                {
                    if (reaction.UserId != attributeDialog.UserId) return;
                    var user = await Client.GetUserAsync(attributeDialog.UserId) as SocketUser;
                    var userProfile = db.LoadRecordById<GameUser>("UserProfiles", attributeDialog.UserId);
                    var serverStats = userProfile.Servers.Where(x => x.ServerId == attributeDialog.ServerId).FirstOrDefault();
                    var attributeAddedText = "";

                    if (serverStats.AvailableAttributePoints <= 0)
                    {
                        attributeAddedText = "You have no available attribute points to spend!";
                    }
                    else
                    {
                        if (reaction.Emote.Name == EmoteHandler.Strength.Name)
                        {
                            serverStats.Attributes.Strength++;
                            attributeAddedText = $"{EmoteHandler.Strength}You increased your Strength by 1!{EmoteHandler.Strength}";
                        }
                        else if (reaction.Emote.Name == EmoteHandler.Intelligence.Name)
                        {
                            serverStats.Attributes.Intelligence++;
                            attributeAddedText = $"{EmoteHandler.Intelligence}You increased your Intelligence by 1!{EmoteHandler.Intelligence}";
                        }
                        else if (reaction.Emote.Name == EmoteHandler.Dexterity.Name)
                        {
                            serverStats.Attributes.Dexterity++;
                            attributeAddedText = $"{EmoteHandler.Dexterity}You increased your Dexterity by 1!{EmoteHandler.Dexterity}";
                        }
                        else if (reaction.Emote.Name == EmoteHandler.Constitution.Name)
                        {
                            serverStats.Attributes.Constitution++;
                            attributeAddedText = $"{EmoteHandler.Constitution}You increased your Constitution by 1!{EmoteHandler.Constitution}";
                            serverStats.CurrentHP += GameServerStats.ConstitutionHPBonus;
                        }
                        else if (reaction.Emote.Name == EmoteHandler.Wisdom.Name)
                        {
                            serverStats.Attributes.Wisdom++;
                            attributeAddedText = $"{EmoteHandler.Wisdom}You increased your Wisdom by 1!{EmoteHandler.Wisdom}";
                            serverStats.CurrentMP += GameServerStats.WisdomMPBonus;
                        }
                        else if (reaction.Emote.Name == EmoteHandler.Luck.Name)
                        {
                            serverStats.Attributes.Luck++;
                            attributeAddedText = $"{EmoteHandler.Luck}You increased your Luck by 1!{EmoteHandler.Luck}";
                        }
                    }
                    db.UpsertRecord("UserProfiles", attributeDialog.UserId, userProfile);
                    await msg.RemoveReactionAsync(reaction.Emote, reaction.UserId);
                    var showAttributesEmbed = await EmbedHandler.CreateAttributeEmbedBuilder(serverStats, user);
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
