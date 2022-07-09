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
            var serverProfile = db.LoadRecordById<GameServerProfile>("GameProfiles", Context.Guild.Id);
            if (serverProfile == null)
            {
                var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find server profile", "It seems this server has no profile, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noServerProfEmbed);
                return;
            }
            var userProfile = serverProfile.Users.Where(x => x.UserId == Context.User.Id).FirstOrDefault();
            if (userProfile == null)
            {
                var noUserProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noUserProfEmbed);
                return;
            }
            if (flag == "")
            {
                var showAttributesEmbed = await EmbedHandler.CreateAttributeEmbedBuilder(userProfile, Context.User);
                var msg = await ReplyAsync(embed: showAttributesEmbed.Build());

                var attributeDialog = new GameAttributeDialog
                {
                    MessageId = msg.Id,
                    ChannelId = Context.Channel.Id,
                    ServerId = Context.Guild.Id,
                    UserId = Context.User.Id,
                    CreationTime = DateTime.UtcNow
                };
                db.InsertRecord("ActiveAttributeDialogs", attributeDialog);
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
            if (amount > userProfile.AvailableAttributePoints)
            {
                var notEnoughPointsEmbed = await EmbedHandler.CreateUserErrorEmbed("Not enough available points!", $"You can't add {amount} points to {attribute}, you only have {userProfile.AvailableAttributePoints} available to spend!");
                await ReplyAsync(embed: notEnoughPointsEmbed);
                return;
            }
            switch (attribute.ToLower())
            {
                case "strength":
                    userProfile.Attributes.Strength += amount;
                    break;
                case "intelligence":
                    userProfile.Attributes.Intelligence += amount;
                    break;
                case "dexterity":
                    userProfile.Attributes.Dexterity += amount;
                    break;
                case "constitution":
                    userProfile.Attributes.Constitution += amount;
                    break;
                case "wisdom":
                    userProfile.Attributes.Wisdom += amount;
                    break;
                case "luck":
                    userProfile.Attributes.Luck += amount;
                    break;
            }
            var successEmbed = await EmbedHandler.CreateBasicEmbed($"{attribute} upgraded!", $"You have added {amount} points to {attribute}");
            await ReplyAsync(embed: successEmbed);
            db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);
        }
    }
}
