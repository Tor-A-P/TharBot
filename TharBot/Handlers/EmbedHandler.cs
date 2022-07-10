using Discord;
using Discord.WebSocket;
using TharBot.DBModels;
using Victoria;

namespace TharBot.Handlers
{
    public static class EmbedHandler
    {
        // Embed descriptions are limited to 4096 characters.

        public static async Task<EmbedBuilder> CreateBasicEmbedBuilder(string title)
        {
            var embedBuilder = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithColor(new Color(76, 164, 210))
                .WithCurrentTimestamp()));
            return embedBuilder;
        }
        public static async Task<Embed> CreateBasicEmbed(string title, string description)
        {
            var embed = await Task.Run(() => (new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(new Color(76, 164, 210))
                .WithCurrentTimestamp().Build()));
            return embed;
        }

        public static async Task<Embed> CreateErrorEmbed(string source, string error)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithTitle($"ERROR OCCURED FROM COMMAND - {source}")
                .AddField("Error Details:", error)
                .WithColor(Color.DarkRed)
                .WithCurrentTimestamp().Build());
            return embed;
        }

        public static async Task<Embed> CreateUserErrorEmbed(string source, string error)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithTitle($"Usage error - {source}")
                .AddField("Error Details:", error)
                .WithColor(Color.DarkRed)
                .WithCurrentTimestamp().Build());
            return embed;
        }

        public static async Task<EmbedBuilder> CreateMusicEmbedBuilder(string title, string description, LavaPlayer player, bool includeQueue = true, bool current = true)
        {
            var embedBuilder = await Task.Run(async () => new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithColor(new Color(76, 164, 210))
            .WithThumbnailUrl(await player.Track.FetchArtworkAsync())
            .WithCurrentTimestamp());

            if (player.Queue.Count > 0 && includeQueue)
            {
                var queue = string.Empty;

                if (current)
                {
                    string currentShortTitle = player.Track.Title.Length > 40 ? player.Track.Title.Substring(0, 40) + "..." : player.Track.Title;
                    queue += $"Current: {currentShortTitle} / {player.Track.Duration}\n\t{player.Track.Url}\n";
                }

                var trackNum = 1;

                foreach (var queuedTrack in player.Queue)
                {
                    if (trackNum > 4) break;
                    string shortTitle = queuedTrack.Title.Length > 40 ? queuedTrack.Title.Substring(0, 40) + "..." : queuedTrack.Title;
                    queue += $"{trackNum}:\t{shortTitle} / {queuedTrack.Duration}\n\t{queuedTrack.Url}\n";
                    trackNum++;
                }

                embedBuilder.AddField("Current queue:", queue);
            }
            return embedBuilder;
        }

        public static async Task<Embed> CreateGameEmbed(GameFight fight, GameUserProfile user, string userName)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                 .WithTitle($"{userName} vs Level {fight.Enemy.Level} {fight.Enemy.Name}!")
                 .AddField($"Lv. {user.Level} {userName}", $"{EmoteHandler.HP}HP:  {user.CurrentHP} / {user.BaseHP}\n" +
                           $"{EmoteHandler.MP}MP:  {user.CurrentMP} / {user.BaseMP}\n" +
                           $"{EmoteHandler.Attack}Atk: {user.BaseAtk}\n" +
                           $"{EmoteHandler.Defend}Def: {user.BaseDef}", true)
                 .AddField($"Lv. {fight.Enemy.Level} {fight.Enemy.Name}", $"{EmoteHandler.HP}HP:  {fight.Enemy.CurrentHP} / {fight.Enemy.BaseHP}\n" +
                           $"{EmoteHandler.MP}MP:  {fight.Enemy.CurrentMP} / {fight.Enemy.BaseMP}\n" +
                           $"{EmoteHandler.Attack}Atk: {fight.Enemy.BaseAtk}\n" +
                           $"{EmoteHandler.Defend}Def: {fight.Enemy.BaseDef}", true)
                 .WithColor(new Color(76, 164, 210))
                 .WithFooter("Click the reactions to do actions like attacking, defending, casting spells, or using consumables"));
            if (fight.Turns != null)
            {
                var turns = fight.Turns.TakeLast(5);
                var i = turns.Count() - 1;
                foreach (var turn in turns)
                {
                    if (turn.Contains("wins the fight"))
                    {
                        embed.AddField("Fight ended!", turn);
                    }
                    else
                    {
                        embed.AddField($"Turn {fight.TurnNumber - i}", turn);
                    }
                    i--;
                }
            }

            return embed.Build();
        }

        public static async Task<EmbedBuilder> CreateAttributeEmbedBuilder(GameUserProfile userProfile, SocketUser user)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                 .WithTitle($"{user}'s attributes:")
                 .AddField($"Available points: {userProfile.AvailableAttributePoints}", $"Total points spent already: {userProfile.SpentAttributePoints}")
                 .AddField($"{EmoteHandler.Strength} Strength: {userProfile.Attributes.Strength}",
                           $"Increases your attack damage by {GameUserProfile.AttackPerStrength} per point.")
                 .AddField($"{EmoteHandler.Intelligence} Intelligence: {userProfile.Attributes.Intelligence}",
                           $"Increases your spell damage by {GameUserProfile.IntSpellModifier}% per point (Not implemented yet).")
                 .AddField($"{EmoteHandler.Dexterity} Dexterity: {userProfile.Attributes.Dexterity}",
                           $"Increases your crit chance by {GameUserProfile.DexCritModifier} and your defense by {GameUserProfile.DexDefModifier} per point.")
                 .AddField($"{EmoteHandler.Constitution} Constitution: {userProfile.Attributes.Constitution}",
                           $"Increases your health by {GameUserProfile.ConstitutionHPBonus} and your health regen per minute by {GameUserProfile.ConstitutionHPRegenBonus}% of your max health per point.")
                 .AddField($"{EmoteHandler.Wisdom} Wisdom: {userProfile.Attributes.Wisdom}",
                           $"Increases your mana by {GameUserProfile.WisdomMPBonus} and your mana regen per minute by {GameUserProfile.WisdomMPRegenBonus}% of your max mana per point.")
                 .AddField($"{EmoteHandler.Luck} Luck: {userProfile.Attributes.Luck}",
                           $"Increases your Critical damage by {GameUserProfile.LuckCritModifier}% per point, and your chance to win the low prize in gambles by 0.33% per point.")
                 .WithThumbnailUrl(user.GetAvatarUrl(ImageFormat.Auto, 2048) ?? user.GetDefaultAvatarUrl())
                 .WithFooter("Use the reactions down below to add 1 point to the specified attribute, if you have available points to spend."));

            return embed;
        }
    }
}
