using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Meme : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;
        private readonly IConfiguration _config;
        private readonly CommandService _service;

        public Meme(CommandService service, IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
            _config = config;
            _service = service;
        }

        [Command("Meme")]
        [Summary("Custom command creation. Creates a new command that repeats what you set it to.\n" +
            "**USAGE:**\n th.meme [COMMAND] [OUTPUT]: creates custom command\n" +
            "th.[COMMAND]: uses existing custom command, outputs [OUTPUT]\n" +
            "th.meme -list: sends a list of all custom commands for this server")]
        [Remarks("Fun")]
        public async Task MemeAsync(string cmdName, [Remainder] string output = "")
        {
            try
            {
                var serverSettings = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
                string? prefix;
                if (serverSettings.Prefix != null) prefix = serverSettings.Prefix;
                else prefix = _config["Prefix"];

                if (cmdName == "-list")
                {
                    var memeList = "";
                    if (serverSettings.Memes == null)
                    {
                        var embed = await EmbedHandler.CreateUserErrorEmbed("No memes found", "This server doesn't seem to have any custom memes yet - try making one!");
                        await ReplyAsync(embed: embed);
                    }
                    else
                    {
                        foreach (var meme in serverSettings.Memes)
                        {
                            memeList += $"\"{meme.Key}\": \"{meme.Value}\"\n";
                        }

                        await File.WriteAllTextAsync("memes.txt", memeList);
                        await Context.Channel.SendFileAsync("memes.txt");
                        File.Delete("memes.txt");
                        return;
                    }
                }

                if (output == "")
                {
                    var embed = await EmbedHandler.CreateUserErrorEmbed("No output for command", "Please enter something for the command to respond, not only a command name");
                    await ReplyAsync(embed: embed);
                    return;
                }

                List<CommandInfo> commands = _service.Commands.ToList();
                foreach (CommandInfo commandInfo in commands)
                {
                    if (commandInfo.Name.ToLower() == cmdName.ToLower() || commandInfo.Aliases.Any(x => x.Equals(cmdName.ToLower())))
                    {
                        var embed = await EmbedHandler.CreateUserErrorEmbed("Command name already exists", "Custom commands can't have the same name as already existing bot commands");
                        await ReplyAsync(embed: embed);
                        return;
                    }
                }

                if (serverSettings.Memes == null)
                {
                    serverSettings.Memes = new Dictionary<string, string>
                    {
                        { cmdName, output }
                    };
                    var update = Builders<ServerSpecifics>.Update.Set(x => x.Memes, serverSettings.Memes);
                    await db.UpsertServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);

                    var embed = await EmbedHandler.CreateBasicEmbed($"Command {cmdName} created!", $"Type {prefix}{cmdName} to use it.");
                    await ReplyAsync(embed: embed);
                }
                else if (serverSettings.Memes.ContainsKey(cmdName))
                {
                    var embed = await EmbedHandler.CreateBasicEmbed($"Command {cmdName} already exists!", $"Type {prefix}{cmdName} to use it.");
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    serverSettings.Memes.Add(cmdName, output);
                    var update = Builders<ServerSpecifics>.Update.Set(x => x.Memes, serverSettings.Memes);
                    await db.UpsertServerAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id, update);

                    var embed = await EmbedHandler.CreateBasicEmbed($"Command {cmdName} created!", $"Type {prefix}{cmdName} to use it.");
                    await ReplyAsync(embed: embed);
                }
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("Meme", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: Meme", null, ex);
            }
        }
    }
}
