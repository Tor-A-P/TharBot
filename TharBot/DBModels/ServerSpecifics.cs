using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class ServerSpecifics
    {
        [BsonId]
        public ulong ServerId { get; set; }
        public List<ulong>? BLChannelId { get; set; }
        public List<ulong>? WLChannelId { get; set; }
        public List<ulong>? GameBLChannelId { get; set; }
        public List<ulong>? GameWLChannelId { get; set; }
        public DailyPulseCheck? DailyPC { get; set; }
        public Dictionary<string, string>? Memes { get; set; }
        public List<Poll>? Polls { get; set; }
        public string? Prefix { get; set; }
        public ulong? PCResultsChannel { get; set; }
        public List<Reminders>? Reminders { get; set; }
        public bool ShowLevelUpMessage { get; set; }
    }
}
