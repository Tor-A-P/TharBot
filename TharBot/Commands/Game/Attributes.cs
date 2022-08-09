using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands.Game
{
    public class Attributes : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public Attributes(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("Attributes")]
        [Alias("Attribute")]
        [Summary("Shows you your current attributes, available attribute points, and lets you add to them if you have any points available.\n" +
            "**USAGE:** th.attributes, th.attributes add [ATTRIBUTE] [AMOUNT]\n" +
            "**EXAMPLE:** th.attributes add strength 5")]
        [Remarks("Game")]
        public async Task AttributesAsync(string flag = "", string attribute = "", int amount = 0)
        {
            try
            {
                var serverSettings = db.LoadRecordById<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
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

                var userProfile = db.LoadRecordById<GameUser>("UserProfiles", Context.User.Id);
                if (userProfile == null)
                {
                    var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                    await ReplyAsync(embed: noServerProfEmbed);
                    return;
                }
                var serverStats = userProfile.Servers.Where(x => x.ServerId == Context.Guild.Id).FirstOrDefault();
                if (serverStats == null)
                {
                    var noUserProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                    await ReplyAsync(embed: noUserProfEmbed);
                    return;
                }
                if (flag == "")
                {
                    var showAttributesEmbed = await EmbedHandler.CreateAttributeEmbedBuilder(serverStats, Context.User);
                    var msg = await ReplyAsync(embed: showAttributesEmbed.Build());

                    var attributeDialog = new GameAttributeDialog
                    {
                        MessageId = msg.Id,
                        ChannelId = Context.Channel.Id,
                        ServerId = Context.Guild.Id,
                        UserId = Context.User.Id,
                        CreationTime = DateTime.UtcNow
                    };
                    if (serverSettings.AttributeDialogs == null)
                    {
                        serverSettings.AttributeDialogs = new List<GameAttributeDialog>();
                    }
                    serverSettings.AttributeDialogs.Add(attributeDialog);
                    db.UpsertRecord("ServerSpecifics", Context.Guild.Id, serverSettings);
                    var emotes = new Emote[]
                    {
                    EmoteHandler.Strength,
                    EmoteHandler.Intelligence,
                    EmoteHandler.Dexterity,
                    EmoteHandler.Constitution,
                    EmoteHandler.Wisdom,
                    EmoteHandler.Luck
                    };
                    await msg.AddReactionsAsync(emotes);
                    return;
                }
                else if (flag.ToLower() != "add")
                {
                    var wrongFlagEmbed = await EmbedHandler.CreateUserErrorEmbed("Wrong command flag!", $"\"{flag}\" is not usable with this command, use \"attributes add [ATTRIBUTE] [AMOUNT]\"");
                    await ReplyAsync(embed: wrongFlagEmbed);
                    return;
                }
                var attributeList = new List<string>
                {
                "strength",
                "intelligence",
                "dexterity",
                "constitution",
                "wisdom",
                "luck"
                };
                if (!attributeList.Contains(attribute))
                {
                    var wrongAttributeEmbed = await EmbedHandler.CreateUserErrorEmbed("Not a valid attribute!", $"{attribute} is not a valid attribute, the allowed attributes are:\n" +
                        $"Strength\n" +
                        $"Intelligence\n" +
                        $"Dexterity\n" +
                        $"Constitution\n" +
                        $"Wisdom\n" +
                        $"Luck");
                    await ReplyAsync(embed: wrongAttributeEmbed);
                    return;
                }
                if (amount <= 0)
                {
                    var wrongAmountEmbed = await EmbedHandler.CreateUserErrorEmbed("Not a valid amount!", $"{amount} is not a valid amount of points to add, enter an integer larger than 0");
                    await ReplyAsync(embed: wrongAmountEmbed);
                    return;
                }
                if (amount > serverStats.AvailableAttributePoints)
                {
                    var notEnoughPointsEmbed = await EmbedHandler.CreateUserErrorEmbed("Not enough available points!", $"You can't add {amount} points to {attribute}, you only have {serverStats.AvailableAttributePoints} available to spend!");
                    await ReplyAsync(embed: notEnoughPointsEmbed);
                    return;
                }
                switch (attribute.ToLower())
                {
                    case "strength":
                        serverStats.Attributes.Strength += amount;
                        break;
                    case "intelligence":
                        serverStats.Attributes.Intelligence += amount;
                        break;
                    case "dexterity":
                        serverStats.Attributes.Dexterity += amount;
                        break;
                    case "constitution":
                        serverStats.Attributes.Constitution += amount;
                        break;
                    case "wisdom":
                        serverStats.Attributes.Wisdom += amount;
                        break;
                    case "luck":
                        serverStats.Attributes.Luck += amount;
                        break;
                }
                var successEmbed = await EmbedHandler.CreateBasicEmbed($"{attribute} upgraded!", $"You have added {amount} points to {attribute}");
                await ReplyAsync(embed: successEmbed);
                db.UpsertRecord("GameProfiles", Context.User.Id, userProfile);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Attributes", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Attributes", null, ex);
            }
        }
    }
}
