using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class BannedUser
    {
        [BsonId]
        public ulong UserId { get; set; }
    }
}
