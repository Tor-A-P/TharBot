using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class BlacklistUser : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _config;

        public BlacklistUser(IConfiguration config)
        {
            _config = config;
        }

        [Command("BlacklistUser")]
        [Alias("blu", "bluser")]
        [Summary("Specifies a user for the bot to ignore or stop ignoring.\n" +
            "**USAGE:** th.blu, th.bluser [USER_ID]\n")]
        [Remarks("Bot Owner")]
        [RequireOwner]
        public async Task BlacklistUserAsync(ulong userId)
        {
            var db = new MongoCRUDHandler("TharBot", _config);
            var banList = db.LoadRecords<BannedUser>("UserBanlist");
            var existingBan = banList.Where(x => x.UserId == userId).FirstOrDefault();
            var bannedUser = await Context.Client.GetUserAsync(userId);

            if (existingBan == null)
            {
                var newBan = new BannedUser
                {
                    UserId = userId,
                };
                db.InsertRecord("UserBanlist", newBan);
                var embed = await EmbedHandler.CreateBasicEmbed("User banned", $"Banned user {bannedUser.Mention} from using the bot!");
                await ReplyAsync(embed: embed);
            }
            else
            {
                db.DeleteRecord<BannedUser>("UserBanlist", existingBan.UserId);
                var embed = await EmbedHandler.CreateBasicEmbed("User unbanned", $"Unbanned user {bannedUser.Mention}, they can now use the bot again! :D");
                await ReplyAsync(embed: embed);
            }
        }
    }
}
