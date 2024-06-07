namespace TharBot.DBModels
{
    public class GameServerStats
    {
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
        public long TharCoins { get; set; }
        public DateTime NextRewards { get; set; }
        public bool GambaInProgress { get; set; }
        public bool FightInProgress { get; set; }
        public double Exp { get; set; }
        public long Level { get; set; }
        public long AttributePoints { get; set; }
        public GameStats? Attributes { get; set; }
        public DateTime LastRespec { get; set; }
        public double CurrentHP { get; set; }
        public double CurrentMP { get; set; }
        public long NumMessages { get; set; }
        public long NumFightsWon { get; set; }
        public DateTime FightPeriodStart { get; set; }
        public int FightsThisHour { get; set; }
        public GameDebuffs? Debuffs { get; set; }
        public long ExMentions { get; set; }
        public double ExpToLevel => (3 * Math.Pow(Level, 2)) + (25 * Level) + 100;
        public double BaseHP => 100 + (Level * 30) + (Attributes.Constitution * ConstitutionHPBonus);
        public double BaseMP => 50 + (Level * 15) + (Attributes.Wisdom * WisdomMPBonus);
        public double BaseAtk => 10 + (Level * 3) + (Attributes.Strength * AttackPerStrength);
        public double BaseDef => 4 + (Level * 1) + Math.Floor(Attributes.Dexterity * DexDefModifier);
        public double CritChance => 10 + (Attributes.Dexterity * DexCritModifier);
        public double CritDamage => 100 + (Attributes.Luck * LuckCritModifier);
        public double SpellPower => 10 + (Attributes.Intelligence * IntSpellPower);
        public double BonusGambaChance => Attributes.Luck * LuckGambaModifier;
        public long SpentAttributePoints => Attributes.Strength + Attributes.Dexterity + Attributes.Intelligence + Attributes.Constitution + Attributes.Wisdom + Attributes.Luck;
        public long AvailableAttributePoints => AttributePoints - SpentAttributePoints;
        public static long AttributePointsPerLevel => 3;
        public static long StartingAttributePoints => 5;
        public static double AttackPerStrength => 2;
        public static double IntSpellPower => 3;
        public static double DexCritModifier => 1;
        public static double DexDefModifier => 0.34;
        public static double ConstitutionHPBonus => 10;
        public static double ConstitutionHPRegenBonus => 0.2;
        public static double WisdomMPBonus => 5;
        public static double WisdomMPRegenBonus => 0.2;
        public static double LuckCritModifier => 3;
        public static double LuckGambaModifier => 0.33;

        public double TotalExp()
        {
            double totalExp = 0;
            for (int i = 1; i < Level; i++)
            {
                totalExp += (3 * Math.Pow(Level, 2)) + (25 * i) + 100;
            }
            totalExp += Exp;
            return totalExp;
        }
    }
}
