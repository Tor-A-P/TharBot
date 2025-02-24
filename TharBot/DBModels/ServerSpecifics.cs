﻿using Discord.WebSocket;
using MongoDB.Bson.Serialization.Attributes;

namespace TharBot.DBModels
{
    public class ServerSpecifics
    {
        [BsonId]
        public ulong ServerId { get; set; }
        public long Revision { get; set; }
        public List<ulong>? BLChannelId { get; set; }
        public List<ulong>? WLChannelId { get; set; }
        public List<ulong>? GameBLChannelId { get; set; }
        public List<ulong>? GameWLChannelId { get; set; }
        public List<GameAttributeDialog>? AttributeDialogs { get; set; }
        public DailyPulseCheck? DailyPC { get; set; }
        public Dictionary<string, string>? Memes { get; set; }
        public List<Poll>? Polls { get; set; }
        public string? Prefix { get; set; }
        public ulong? PCResultsChannel { get; set; }
        public List<Reminders>? Reminders { get; set; }
        public bool ShowLevelUpMessage { get; set; }
        public ulong LastChannelUsedId { get; set; }
        public string? ReplaceTwitterLinks { get; set; }
        public string? ReplaceInstagramLinks { get; set; }
        public string? ReplacePixivLinks { get; set; }
        public long? LastExMention { get; set; }
    }
}
