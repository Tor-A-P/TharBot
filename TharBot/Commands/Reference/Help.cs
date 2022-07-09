using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;

        public Help(CommandService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", configuration);
        }

        [Command("Help")]
        [Alias("H")]
        [Summary("Displays every command and their descriptions\n" +
                "**USAGE:** th.help")]
        [Remarks("Reference")]
        public async Task HelpAsync([Remainder] string? command = null)
        {
            List<CommandInfo> commands = _service.Commands.ToList();
            var embedBuilder = new EmbedBuilder().WithColor(new Color(76, 164, 210));
            var existingPrefix = db.LoadRecordById<Prefixes>("Prefixes", Context.Guild.Id);

            if (command != null)
            {
                foreach (CommandInfo commandInfo in commands)
                {
                    if (commandInfo.Name.ToLower() == command.ToLower() || commandInfo.Aliases.Any(x => x.ToLower().Equals(command.ToLower())))
                    {
                        string embedFieldText = "";
                        if (existingPrefix != null)
                        {
                            embedFieldText = commandInfo.Summary.Replace("th.", existingPrefix.Prefix ?? _configuration["Prefix"]) ?? "No description available\n";
                        }
                        else
                        {
                            embedFieldText = commandInfo.Summary.Replace("th.", _configuration["Prefix"]) ?? "No description available\n";
                        }
                        
                        embedBuilder.WithTitle(commandInfo.Name);
                        embedBuilder.AddField("Aliases", string.Join(", ", commandInfo.Aliases));
                        embedBuilder.AddField("Summary", embedFieldText);
                    }
                }
            }
            else
            {
                embedBuilder.AddField("Fun", PopulateCategory(commands, "Fun"));
                embedBuilder.AddField("Game", PopulateCategory(commands, "Game"));
                embedBuilder.AddField("Info", PopulateCategory(commands, "Info"));
                embedBuilder.AddField("Music", PopulateCategory(commands, "Music"));
                embedBuilder.AddField("Reference", PopulateCategory(commands, "Reference"));
                embedBuilder.AddField("Utility", PopulateCategory(commands, "Utility"));
                embedBuilder.AddField("Admin", PopulateCategory(commands, "Admin"));
                embedBuilder.AddField("Setup (requires manage channels permission)", PopulateCategory(commands, "Setup"));

                if (Context.User.Id == 212161497256689665)
                {
                    embedBuilder.AddField("Bot Owner (SOOPER SEKRIT DONUT LOOK)", PopulateCategory(commands, "Bot Owner"));
                }

                if(existingPrefix != null)
                {
                    embedBuilder.WithTitle("Available Commands")
                    .WithFooter($"For help with a specific command, use \"{existingPrefix.Prefix ?? _configuration["Prefix"]}help [COMMAND]\"");
                }
                else
                {
                    embedBuilder.WithTitle("Available Commands")
                    .WithFooter($"For help with a specific command, use \"{_configuration["Prefix"]}help [COMMAND]\"");
                }
            }
            var embed = embedBuilder.Build();

            if (embed.Title == null) await ReplyAsync($"Could not find command \"{command}\"!");
            else await ReplyAsync(embed: embed);
        }

        private static string PopulateCategory(List<CommandInfo> commands, string cat)
        {
            var result = "";
            foreach (CommandInfo commandInfo in commands)
            {
                if (commandInfo.Remarks == null) continue;
                if (commandInfo.Remarks.ToLower() == cat.ToLower())
                {
                    result += commandInfo.Name + ", ";
                }
            }
            result = result.Remove(result.Length - 2, 2);
            return result;
        }
    }
}
