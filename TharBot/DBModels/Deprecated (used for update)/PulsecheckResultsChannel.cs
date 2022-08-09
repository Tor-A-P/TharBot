using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class PulseCheckResultsChannel
    {
        [BsonId]
        public ulong ServerId { get; set; }
        public ulong ResultsChannel { get; set; }
    }
}
