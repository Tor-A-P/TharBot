using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class Reminders
    {
        [BsonId]
        public Guid Id { get; set; }
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTime RemindingTime { get; set; }
        public string ReminderText { get; set; }
    }
}
