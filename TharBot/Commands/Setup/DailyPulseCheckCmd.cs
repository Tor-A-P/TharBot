using Discord.Commands;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using MongoDB.Driver;
using System.Text.RegularExpressions;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class DailyPulseCheckCmd : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public DailyPulseCheckCmd(Microsoft.Extensions.Configuration.IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("DailyPulseCheck")]
        [Alias("Dailypc")]
        [Summary("Sets up a time for the bot to run a pulsecheck (th.pulsecheck) every day, in the channel this command is used.\n" +
            "Optional flags for how long you want the pulsecheck to last (default is 6 hours), whether you want the bot to do an @here ping when posting the pulsecheck (default is false)," +
            "and whether you want the bot to do pulsechecks during weekends as well as weekdays (default is false).\n" +
            "**USAGE:** th.dailypulsecheck [TIME] [OPTIONAL_DURATION_MINUTES] [FLAG_FOR_@HERE] [FLAG_FOR_WEEKEND]\n" +
            "**EXAMPLES:**\n" +
            "th.dailypc midnight 600 false true // Bot will create a pulsecheck at midnight every day, including weekends, which will last 600 minutes and not ping when posting\n" +
            "th.dailypc 12pm // Bot will create a pulsecheck at 12pm every day except weekends, it will last 6 hours/360 minutes, and it will not ping when posting.\n" +
            "th.dailypc // No arguments will check if this server has a pulsecheck scheduled daily, and cancel it if that is the case.")]
        [Remarks("Setup")]
        [RequireUserPermission(Discord.ChannelPermission.ManageChannels, Group = "Permission")]
        [RequireOwner(Group = "Permission")]
        public async Task DailyPulseCheckAsync(string time = "", int duration = 360, bool ping = false, bool weekends = false)
        {
            try
            {
                var serverSpecifics = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
                if (time == "")
                {
                    if (serverSpecifics.DailyPC != null)
                    {
                        serverSpecifics.DailyPC = null;
                        var update = Builders<ServerSpecifics>.Update.Set(x => x.DailyPC, serverSpecifics.DailyPC);
                        await db.UpsertServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
                        var deletedEmbed = await EmbedHandler.CreateBasicEmbed("Daily pulsecheck cancelled!", "Removed the currently scheduled task for a daily pulsecheck for this server!");
                        await ReplyAsync(embed: deletedEmbed);
                    }
                    else
                    {
                        var nonExistingEmbed = await EmbedHandler.CreateBasicEmbed("Could not cancel daily pulsecheck!", "There is no daily pulsecheck currently set up for this server yet.");
                        await ReplyAsync(embed: nonExistingEmbed);
                    }
                }
                else
                {
                    var aiResult = DateTimeRecognizer.RecognizeDateTime(time, Culture.English);
                    if (aiResult.Count == 0)
                    {
                        var failedParseEmbed = await EmbedHandler.CreateUserErrorEmbed("Couldn't parse time", $"\"{time}\" wasn't recognized as a time interval, please try again!");
                        await ReplyAsync(embed: failedParseEmbed);
                        return;
                    }

                    var dictResult = (aiResult.First().Resolution["values"] as List<Dictionary<string, string>>)[0];

                    string type = dictResult["type"];
                    if (!(new string[] { "datetime", "date", "time", "datetimerange", "daterange", "timerange" }).Contains(type))
                    {
                        var typeMismatchEmbed = await EmbedHandler.CreateErrorEmbed("DailyPulseCheck", $"Invalid type of {type} was encountered (\"datetime\" expected). Please message Tharwatha#5189 with what you typed in to get this error message!");
                        await ReplyAsync(embed: typeMismatchEmbed);
                        return;
                    }

                    string result = Regex.IsMatch(type, @"range$") ? dictResult["start"] : dictResult["value"];
                    if (DateTime.TryParse(result, out DateTime dateTime))
                    {
                        var newDailyPC = new DailyPulseCheck
                        {
                            ServerId = Context.Guild.Id,
                            ChannelId = Context.Channel.Id,
                            WhenToRun = dateTime,
                            LastTimeRun = DateTime.MinValue,
                            Duration = duration,
                            ShouldPing = ping,
                            OnWeekends = weekends
                        };
                        
                        if (serverSpecifics.DailyPC != null)
                        {
                            serverSpecifics.DailyPC = newDailyPC;
                            var update = Builders<ServerSpecifics>.Update.Set(x => x.DailyPC, serverSpecifics.DailyPC);
                            await db.UpsertServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
                            var updatedEmbed = await EmbedHandler.CreateBasicEmbed("Daily pulsecheck task updated!",
                                $"The daily pulsecheck time has been changed to {newDailyPC.WhenToRun.ToShortTimeString()} in this channel, and will last {duration} minutes.\n" +
                                $"Should it ping @here? {ping}\n" +
                                $"Should it post during weekends? {weekends}");
                            await ReplyAsync(embed: updatedEmbed);
                        }
                        else
                        {
                            serverSpecifics.DailyPC = newDailyPC;
                            var update = Builders<ServerSpecifics>.Update.Set(x => x.DailyPC, serverSpecifics.DailyPC);
                            await db.UpsertServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);
                            var createdEmbed = await EmbedHandler.CreateBasicEmbed("Daily pulsecheck task created!",
                                $"From now on I will post a pulsecheck daily in this channel at {newDailyPC.WhenToRun.ToShortTimeString()} and it will last {duration} minutes.\n" +
                                $"Should it ping @here? {ping}\n" +
                                $"Should it post during weekends? {weekends}");
                            await ReplyAsync(embed: createdEmbed);
                        }
                    }
                    else
                    {
                        var failedParseEmbed = await EmbedHandler.CreateUserErrorEmbed("Couldn't parse time", $"\"{time}\" wasn't recognized as a time interval, please try again!");
                        await ReplyAsync(embed: failedParseEmbed);
                    }
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("DailyPulseCheck", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: DailyPC", null, ex);
            }
        }
    }
}
