using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{

    public class DebugCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IConfiguration _configuration;
        private readonly MongoCRUDHandler db;

        public DebugCommands(IConfiguration configuration)
        {
            _configuration = configuration;
            db = new MongoCRUDHandler("TharBot", configuration);
        }

        [Command("Ping")]
        [Summary("Simply returns \"Pong!\"\n**USAGE:** th.ping")]
        [Remarks("Test Commands")]
        public async Task PingAsync()
        {
            var bot = Context.Client;
            await Context.Channel.TriggerTypingAsync();
            await Context.Channel.SendMessageAsync($"Pong! {bot.Latency}ms");
        }

        [Command("StunMe")]
        [Summary("Testing the debuff system")]
        [Remarks("Test Commands")]
        [RequireOwner]
        public async Task StunMeAsync(int rounds = 1)
        {
            var userProfile = await db.LoadRecordByIdAsync<GameUser>("UserProfiles", Context.User.Id);
            if (userProfile == null)
            {
                var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noServerProfEmbed);
                return;
            }
            var serverStats = userProfile.Servers.Where(x => x.ServerId == Context.Guild.Id).FirstOrDefault();
            if (serverStats == null)
            {
                var noUserProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noUserProfEmbed);
                return;
            }
            serverStats.Debuffs.StunDuration = rounds;
            await db.UpsertRecordAsync("UserProfiles", Context.User.Id, userProfile);
            var embed = await EmbedHandler.CreateBasicEmbed("Stunned you!", $"Stunned you for {rounds} rounds.");
            await ReplyAsync(embed: embed);
        }

        [Command("HealMe")]
        [Summary("Debug command to heal user")]
        [Remarks("Test Commands")]
        [RequireOwner]
        public async Task HealMeAsync()
        {
            await Task.Delay(2000);
            var userProfile = await db.LoadRecordByIdAsync<GameUser>("UserProfiles", Context.User.Id);
            if (userProfile == null)
            {
                var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noServerProfEmbed);
                return;
            }
            var serverStats = userProfile.Servers.Where(x => x.ServerId == Context.Guild.Id).FirstOrDefault();
            if (serverStats == null)
            {
                var noUserProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noUserProfEmbed);
                return;
            }
            serverStats.CurrentHP = serverStats.BaseHP;
            await db.UpsertRecordAsync("UserProfiles", Context.User.Id, userProfile);
            var embed = await EmbedHandler.CreateBasicEmbed("Healed you!", "Returned you to full base health!");
            await ReplyAsync(embed: embed);
        }
    }
}