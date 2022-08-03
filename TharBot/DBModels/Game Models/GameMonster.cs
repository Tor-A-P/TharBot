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
        public double BaseHP => 100 + (Level * 30) + (Stats.Constitution * GameUserProfile.ConstitutionHPBonus);
        public double BaseMP => 50 + (Level * 15) + (Stats.Wisdom * GameUserProfile.WisdomMPBonus);
        public double BaseAtk => 10 + (Level * 3) + (Stats.Strength * GameUserProfile.AttackPerStrength);
        public double BaseDef => 4 + (Level * 1) + Math.Floor(Stats.Dexterity / GameUserProfile.DexDefModifier);
        public double CritChance => 10 + (Stats.Dexterity * GameUserProfile.DexCritModifier);
        public double CritDamage => 100 + (Stats.Luck * GameUserProfile.LuckCritModifier);
        public double SpellPower => 10 + (Stats.Intelligence * GameUserProfile.IntSpellPower);
    }
}
