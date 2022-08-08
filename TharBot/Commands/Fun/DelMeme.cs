﻿using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class DelMeme : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;
        private readonly IConfiguration _config;


        public DelMeme(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
            _config = config;
        }

        [Command("Deletememe")]
        [Alias("dmeme", "delmeme")]
        [Summary("Deletes a custom command created with th.meme\n" +
            "**USAGE:** -dmeme [COMMAND_NAME]")]
        [Remarks("Fun")]
        public async Task DelMemeAsync(string cmdName)
        {
            var serverSettings = db.LoadRecordById<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);

            if (serverSettings.Memes == null)
            {
                var embed = await EmbedHandler.CreateUserErrorEmbed($"Meme {cmdName} could not be deleted",
                    $"Couldn't find a custom command for this server with the name {cmdName}, are you sure it exists?");
                await ReplyAsync(embed: embed);
            }
            else if (!serverSettings.Memes.ContainsKey(cmdName))
            {
                var embed = await EmbedHandler.CreateUserErrorEmbed($"Meme {cmdName} could not be deleted",
                    $"Couldn't find a custom command for this server with the name {cmdName}, are you sure it exists?");
                await ReplyAsync(embed: embed);
            }
            else
            {
                serverSettings.Memes.Remove(cmdName);
                db.UpsertRecord("ServerSpecifics", Context.Guild.Id, serverSettings);

                var embed = await EmbedHandler.CreateBasicEmbed($"{cmdName} removed!", $"Removed custom command named {cmdName}");
                await ReplyAsync(embed: embed);
            }
        }
    }
}
