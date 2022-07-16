using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
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
                var existingMemeList = db.LoadRecordById<MemeCommands>("Memes", Context.Guild.Id);

                if (cmdName == "-list")
                {
                    var memeList = "";
                    if (existingMemeList == null)
                    {
                        var embed = await EmbedHandler.CreateUserErrorEmbed("No memes found", "This server doesn't seem to have any custom memes yet - try making one!");
                        await ReplyAsync(embed: embed);
                    }
                    else
                    {
                        foreach (var meme in existingMemeList.Memes)
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

                if (existingMemeList == null)
                {
                    var newMemeDict = new Dictionary<string, string>
                    {
                        { cmdName, output }
                    };
                    var newMemeList = new MemeCommands
                    {
                        ServerId = Context.Guild.Id,
                        Memes = newMemeDict
                    };
                    db.InsertRecord("Memes", newMemeList);

                    var embedText = MemeEmbedText(cmdName);
                    var embed = await EmbedHandler.CreateBasicEmbed($"Command {cmdName} created!", embedText);
                    await ReplyAsync(embed: embed);
                }
                else if (existingMemeList.Memes.ContainsKey(cmdName))
                {
                    var embedText = MemeEmbedText(cmdName);
                    var embed = await EmbedHandler.CreateBasicEmbed($"Command {cmdName} already exists!", embedText);
                    await ReplyAsync(embed: embed);
                }
                else
                {
                    existingMemeList.Memes.Add(cmdName, output);
                    db.UpsertRecord("Memes", Context.Guild.Id, existingMemeList);

                    var embedText = MemeEmbedText(cmdName);
                    var embed = await EmbedHandler.CreateBasicEmbed($"Command {cmdName} created!", embedText);
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

        public string MemeEmbedText(string cmd)
        {
            var existingPrefix = db.LoadRecordById<Prefixes>("Prefixes", Context.Guild.Id);
            string embedText;
            string prefix;
            if (existingPrefix != null) prefix = existingPrefix.Prefix;
            else prefix = _config["Prefix"];
            embedText = $"Type {prefix}{cmd} to use it.";
            return embedText;
        }
    }
}
