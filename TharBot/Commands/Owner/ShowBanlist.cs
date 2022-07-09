using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class ShowBanlist : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _config;

        public ShowBanlist(IConfiguration config)
        {
            _config = config;
        }

        [Command("Banlist")]
        [Alias("Showbanlist")]
        [Summary("Shows the list of banned users, both mentioning them and showing their ID\n" +
            "**USAGE:** th.banlist, th.showbanlist")]
        [Remarks("Bot Owner")]
        [RequireOwner]
        public async Task ShowBanlistAsync()
        {
            var db = new MongoCRUDHandler("TharBot", _config);

            var banlist = db.LoadRecords<BannedUser>("UserBanlist");
            var embed = await EmbedHandler.CreateBasicEmbedBuilder("Users banned from using TharBot");
            if (banlist.Count > 0)
            {
                foreach (var bannedUser in banlist)
                {
                    var user = await Context.Client.GetUserAsync(bannedUser.UserId);
                    embed.AddField(user.Id.ToString(), user.Mention, true);
                }
            }
            else
            {
                embed.Description = "No users banned! Everyone's on their best behavior! :D";
            }

            await ReplyAsync(embed: embed.Build());
        }
    }
}
