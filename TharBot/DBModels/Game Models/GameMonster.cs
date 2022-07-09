using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class GameMonster
    {
        [BsonId]
        public string Name { get; set; }
        public long Level { get; set; }
        public GameStats? Stats { get; set; }
        public long MinLevel { get; set; }
        public double CurrentHP { get; set; }
        public double CurrentMP { get; set; }
        public double BaseHP => 100 + (Level * 30) + (Stats.Constitution * 10);
        public double BaseMP => 50 + (Level * 15) + (Stats.Wisdom * 5);
        public double BaseAtk => 10 + (Level * 3) + Stats.Strength;
        public double BaseDef => 4 + (Level * 1) + (Stats.Dexterity / 3);
        public double CritChance => 10 + Stats.Dexterity;
        public double CritDamage => 100 + Stats.Luck * 3;
    }
}
