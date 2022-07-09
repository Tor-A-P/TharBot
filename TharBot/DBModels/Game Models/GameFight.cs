using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class GameFight
    {
        [BsonId]
        public ulong MessageId { get; set; }
        public DateTime LastMoveTime { get; set; }
        public List<string>? Turns { get; set; }
        public int TurnNumber { get; set; }
        public GameMonster Enemy { get; set; }
        public ulong ServerId { get; set; }
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
        public static TimeSpan LifeTime => TimeSpan.FromMinutes(5);
    }
}
