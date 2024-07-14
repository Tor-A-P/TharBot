using Discord.Commands;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class Invite : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;

        public Invite(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [Command("Invite")]
        [Summary("Gives you a link to invite the bot to any servers you have management permissions in.\n" +
            "**USAGE:** th.invite\n")]
        [Remarks("Setup")]
        public async Task PrefixAsync()
        {
            if (Context.User.IsBot) return;

            var embed = await EmbedHandler.CreateBasicEmbed("Invite the bot to your server!", $"Use [this link]({_configuration["InviteLink"]}) to invite the bot to your server!\n" +
                $"The bot will have the default prefix of \"{_configuration["Prefix"]}\", use the prefix command to change it to your liking once you've invited it (for example \"{_configuration["Prefix"]}prefix !\" if you want the command prefix to be !)");

            await ReplyAsync(embed: embed);
        }
    }
}
