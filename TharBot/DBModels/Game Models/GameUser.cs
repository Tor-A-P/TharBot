using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class GameUser
    {
        [BsonId]
        public ulong UserId { get; set; }
        public List<GameServerStats> Servers { get; set; }
        public long Revision { get; set; }
        public string? LastSeenUsername { get; set; }
    }
}
