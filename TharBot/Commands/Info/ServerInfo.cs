using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class ServerInfo : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IConfiguration _config;
        private readonly MongoCRUDHandler db;

        public ServerInfo(DiscordSocketClient client, IConfiguration config)
        {
            _client = client;
            db = new MongoCRUDHandler("TharBot", config);
            _config = config;
        }

        [Command("Serverinfo")]
        [Alias("Guildinfo")]
        [Summary("Returns information about this server, or its roles.\n" +
                "**USAGE:** th.serverinfo, th.serverinfo roles")]
        [Remarks("Info")]
        public async Task ServerInfoAsync(string flag = "")
        {
            var guild = Context.Guild;
            var serverSpecifics = await db.LoadRecordByIdAsync<ServerSpecifics>("ServerSpecifics", Context.Guild.Id);
            var prefix = "";
            if (serverSpecifics.Prefix == null) prefix = _config["Prefix"];
            else prefix = serverSpecifics.Prefix;

            if (flag == "")
            {
                var verificationLevelText = "";
                switch (guild.VerificationLevel)
                {
                    case VerificationLevel.None:
                        verificationLevelText = "Unrestricted.";
                        break;
                    case VerificationLevel.Low:
                        verificationLevelText = "Must have a verified email.";
                        break;
                    case VerificationLevel.Medium:
                        verificationLevelText = "Must be registered on Discord for 5+ minutes.";
                        break;
                    case VerificationLevel.High:
                        verificationLevelText = "Must be a member of this server for 10+ minutes.";
                        break;
                    case VerificationLevel.Extreme:
                        verificationLevelText = "Must have a verified phone on Discord.";
                        break;
                    default:
                        verificationLevelText = "Unknown.";
                        break;
                }
                var embedBuilder = await EmbedHandler.CreateBasicEmbedBuilder($"Info about {guild.Name}");

                var embed = embedBuilder
                    .AddField("ID", guild.Id)
                    .AddField("Verification Level", $"{guild.VerificationLevel} - {verificationLevelText}")
                    .AddField("Members", guild.MemberCount)
                    .AddField($"Channels ({guild.Channels.Count + guild.VoiceChannels.Count})", $"Text Channels: {guild.TextChannels.Count}\n" +
                                                                    $"Voice channels: {guild.VoiceChannels.Count}\n\n" +
                                                                    $"Threads: { guild.ThreadChannels.Count}\n" +
                                                                    $"Categories: {guild.CategoryChannels.Count}")
                    .AddField("Server owner", guild.Owner.Mention)
                    .AddField("Created on", TimestampTag.FromDateTimeOffset(guild.CreatedAt))
                    .AddField("Roles", $"To see a list of all roles, use `{prefix}serverinfo roles`")
                    .WithThumbnailUrl(guild.IconUrl)
                    .Build();

                await ReplyAsync(embed: embed);
            }
            else if (flag.ToLower() == "roles")
            {
                var roles = "";
                foreach (var role in guild.Roles.OrderByDescending(x => x.Position))
                {
                    if (role.Name == "@everyone") continue;
                    roles += role.Mention + ", ";
                }
                if (roles != "") roles = roles.Remove(roles.Length - 2, 2);
                else roles = "None";
                var embed = await EmbedHandler.CreateBasicEmbed($"Roles ({guild.Roles.Count - 1})", roles);
                await ReplyAsync(embed: embed);
            }
            else
            {
                var embed = await EmbedHandler.CreateUserErrorEmbed("Wrong keyword used", $"\"{flag}\" is an invalid keyword, this command only accepts the keyword \"roles\"");
                await ReplyAsync(embed: embed);
            }
            
        }
    }
}
