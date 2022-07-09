using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class Blacklist
    {
        [BsonId]
        public ulong ServerId { get; set; }
        public List<ulong> BLChannelId { get; set; }
    }
}
