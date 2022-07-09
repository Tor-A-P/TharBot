using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class Poll
    {
        [BsonId]
        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
        public List<ActivePollResponse>? Responses { get; set; }
        public List<string>? Emojis { get; set; }
        public DateTime CreationTime { get; set; }
        public TimeSpan LifeSpan { get; set; }
        public DateTime CompletionTime { get; set; }
        public int NumOptions { get; set; }
    }

    public class ActivePollResponse
    {
        public ulong VoterId { get; set; }
        public string? Vote { get; set; }
    }
}
