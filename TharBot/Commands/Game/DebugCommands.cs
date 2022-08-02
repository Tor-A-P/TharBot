﻿using Discord.Commands;
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
            var serverProfile = db.LoadRecordById<GameServerProfile>("GameProfiles", Context.Guild.Id);
            if (serverProfile == null)
            {
                var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find server profile", "It seems this server has no profile, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noServerProfEmbed);
                return;
            }
            var userProfile = serverProfile.Users.Where(x => x.UserId == Context.User.Id).FirstOrDefault();
            if (userProfile == null)
            {
                var noUserProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noUserProfEmbed);
                return;
            }
            userProfile.Debuffs.StunDuration = rounds;
            db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);
            var embed = await EmbedHandler.CreateBasicEmbed("Stunned you!", $"Stunned you for {rounds} rounds.");
            await ReplyAsync(embed: embed);
        }

        [Command("HealMe")]
        [Summary("Debug command to heal user")]
        [Remarks("Test Commands")]
        [RequireOwner]
        public async Task HealMeAsync()
        {
            var serverProfile = db.LoadRecordById<GameServerProfile>("GameProfiles", Context.Guild.Id);
            if (serverProfile == null)
            {
                var noServerProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find server profile", "It seems this server has no profile, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noServerProfEmbed);
                return;
            }
            var userProfile = serverProfile.Users.Where(x => x.UserId == Context.User.Id).FirstOrDefault();
            if (userProfile == null)
            {
                var noUserProfEmbed = await EmbedHandler.CreateUserErrorEmbed("Could not find user profile", "It seems you have no profile on this server, try sending a message (not a command) and then use this command again!");
                await ReplyAsync(embed: noUserProfEmbed);
                return;
            }
            userProfile.CurrentHP = userProfile.BaseHP;
            db.UpsertRecord("GameProfiles", Context.Guild.Id, serverProfile);
            var embed = await EmbedHandler.CreateBasicEmbed("Healed you!", "Returned you to full base health!");
            await ReplyAsync(embed: embed);
        }
    }
}