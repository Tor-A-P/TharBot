using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class GameAttributeDialog
    {
        [BsonId]
        public ulong MessageId { get; set; }
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
        public ulong ServerId { get; set; }
        public DateTime CreationTime { get; set; }
        public static TimeSpan LifeTime => TimeSpan.FromMinutes(5);
    }
}
