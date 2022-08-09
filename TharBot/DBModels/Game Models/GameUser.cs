using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class GameUser
    {
        [BsonId]
        public ulong UserId { get; set; }
        public List<GameServerStats> Servers { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? LastSeenUsername { get; set; }
    }
}
