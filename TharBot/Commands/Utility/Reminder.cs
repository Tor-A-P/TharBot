using Discord.Commands;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using System.Text.RegularExpressions;
using TharBot.DBModels;
using TharBot.Handlers;



namespace TharBot.Commands
{
    public class Reminder : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public Reminder(Microsoft.Extensions.Configuration.IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("Reminder")]
        [Summary("Lets you set a reminder, making the bot ping you some time in the future.\n" +
            "**USAGE:** th.reminder [REMINDER_TEXT] in [TIME_INTERVAL]\n" +
            "**EXAMPLE:** th.reminder pick up kids in 4 days 6 hours, th.reminder will the bot even be running anymore? in june 2025\n")]
        [Remarks("Utility")]
        public async Task ReminderAsync([Remainder] string input)
        {
            try
            {
                var splitIndex = input.LastIndexOf(" in ");
                var reminderText = input.Substring(0, splitIndex);
                var timeString = input.Substring(splitIndex + 1).Trim();
                var aiResult = DateTimeRecognizer.RecognizeDateTime(timeString, Culture.English);
                if (aiResult.Count == 0)
                {
                    var failedParseEmbed = await EmbedHandler.CreateUserErrorEmbed("Couldn't parse time", $"\"{timeString}\" wasn't recognized as a time interval, please try again!");
                    await ReplyAsync(embed: failedParseEmbed);
                    return;
                }

                var dictResult = (aiResult.First().Resolution["values"] as List<Dictionary<string, string>>)[0];

                string type = dictResult["type"];
                if (!(new string[] { "datetime", "date", "time", "datetimerange", "daterange", "timerange" }).Contains(type))
                {
                    var typeMismatchEmbed = await EmbedHandler.CreateErrorEmbed("Reminder", $"Invalid type of {type} was encountered (\"datetime\" expected). Please message Tharwatha#5189 with what you typed in to get this error message!");
                    await ReplyAsync(embed: typeMismatchEmbed);
                    return;
                }

                string result = Regex.IsMatch(type, @"range$") ? dictResult["start"] : dictResult["value"];
                if (DateTime.TryParse(result, out DateTime dateTime))
                {
                    var newReminder = new Reminders
                    {
                        Id = new Guid(),
                        ReminderText = reminderText,
                        ChannelId = Context.Channel.Id,
                        UserId = Context.User.Id,
                        RemindingTime = dateTime
                    };
                    db.InsertRecord("Reminders", newReminder);
                    var embed = await EmbedHandler.CreateBasicEmbed("Reminder created!", $"I will remind you to {reminderText} at {dateTime:f}");
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    var failedParseEmbed = await EmbedHandler.CreateUserErrorEmbed("Couldn't parse time", $"\"{timeString}\" wasn't recognized as a time interval, please try again!");
                    await ReplyAsync(embed: failedParseEmbed);
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Reminder", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Reminder", null, ex);
            }
            
        }
    }
}
