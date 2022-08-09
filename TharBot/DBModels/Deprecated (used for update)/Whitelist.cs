using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class Whitelist
    {
        [BsonId]
        public ulong ServerId { get; set; }
        public List<ulong> WLChannelId { get; set; }
    }
}
