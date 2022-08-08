using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class MemeCommands
    {
        [BsonId]
        public ulong ServerId { get; set; }

        public Dictionary<string, string>? Memes { get; set; }
    }
}
