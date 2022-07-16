using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class Prefixes
    {
        [BsonId]
        public ulong ServerId { get; set; }
        public string Prefix { get; set; }
    }
}
