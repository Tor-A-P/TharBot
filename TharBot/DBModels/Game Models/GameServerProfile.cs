using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class GameServerProfile
    {
        [BsonId]
        public ulong ServerId { get; set; }
        public List<GameUserProfile> Users { get; set; }
    }
}
