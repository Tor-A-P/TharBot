namespace TharBot.DBModels
{
    public class DailyPulseCheck
    {
        public ulong ServerId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTime LastTimeRun { get; set; }
        public DateTime WhenToRun { get; set; }
        public int Duration { get; set; }
        public bool ShouldPing { get; set; }
        public bool OnWeekends { get; set; }
    }
}