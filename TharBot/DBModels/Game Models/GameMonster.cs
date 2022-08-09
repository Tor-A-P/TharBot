using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class GameMonster
    {
        [BsonId]
        public string? Name { get; set; }
        public long Level { get; set; }
        public GameStats? Stats { get; set; }
        public long MinLevel { get; set; }
        public double CurrentHP { get; set; }
        public double CurrentMP { get; set; }
        public GameDebuffs? Debuffs { get; set; }
        public double BaseHP => 100 + (Level * 30) + (Stats.Constitution * GameServerStats.ConstitutionHPBonus);
        public double BaseMP => 50 + (Level * 15) + (Stats.Wisdom * GameServerStats.WisdomMPBonus);
        public double BaseAtk => 10 + (Level * 3) + (Stats.Strength * GameServerStats.AttackPerStrength);
        public double BaseDef => 4 + (Level * 1) + Math.Floor(Stats.Dexterity * GameServerStats.DexDefModifier);
        public double CritChance => 10 + (Stats.Dexterity * GameServerStats.DexCritModifier);
        public double CritDamage => 100 + (Stats.Luck * GameServerStats.LuckCritModifier);
        public double SpellPower => 10 + (Stats.Intelligence * GameServerStats.IntSpellPower);
    }
}
