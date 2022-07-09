using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using TharBot.DBModels;
using TharBot.Handlers;

namespace TharBot.Commands
{
    public class CreateMonster : ModuleBase<SocketCommandContext>
    {
        private readonly MongoCRUDHandler db;

        public CreateMonster(IConfiguration config)
        {
            db = new MongoCRUDHandler("TharBot", config);
        }

        [Command("CreateMonster")]
        [Summary("Creates a new monster that can be generated in fights.\n" +
            "**USAGE:** th.createmonster [MONSTER_NAME] [MIN_LEVEL] [MONSTER_STR] [MONSTER_DEX] [MONSTER INT] [MONSTER CON] [MONSTER WIS] [MONSTER LUK]")]
        [Remarks("Bot Owner")]
        [RequireOwner]
        public async Task CreateMonsterAsync(string name, int minLevel, int str, int dex, int intelligence, int con, int wis, int luk)
        {
            try
            {
                var oldMon = db.LoadRecordById<GameMonster>("MonsterList", name);
                if (oldMon != null)
                {
                    var monsterExistsEmbed = await EmbedHandler.CreateUserErrorEmbed("Monster exists already!", $"There's already a monster named {name} in the database, be more creative!");
                    await ReplyAsync(embed: monsterExistsEmbed);
                    return;
                }

                var newMon = new GameMonster
                {
                    Name = name,
                    MinLevel = minLevel,
                    Stats = new GameStats
                    {
                        Strength = str,
                        Dexterity = dex,
                        Intelligence = intelligence,
                        Constitution = con,
                        Wisdom = wis,
                        Luck = luk
                    }
                };
                db.InsertRecord("MonsterList", newMon);
                var monsterAddedEmbed = await EmbedHandler.CreateBasicEmbed($"Added monster \"{name}\" to the database",
                    $"{name} can be encountered after level {minLevel}, and has the following stats:\n" +
                    $"Strength: {str}\n" +
                    $"Dexterity: {dex}\n" +
                    $"Intelligence: {intelligence}\n" +
                    $"Constitution: {con}\n" +
                    $"Wisdom: {wis}\n" +
                    $"Luck: {luk}");
                await ReplyAsync(embed: monsterAddedEmbed);
            }
            catch (Exception ex)
            {
                var exEmbed = await EmbedHandler.CreateErrorEmbed("CreateMonster", ex.Message);
                await ReplyAsync(embed: exEmbed);
                await LoggingHandler.LogCriticalAsync("COMND: CreateMonster", null, ex);
            }
        }
    }
}
