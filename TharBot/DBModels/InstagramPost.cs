using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TharBot.DBModels
{
    public class InstagramPost
    {
        [BsonId]
        public ulong MessageId { get; set; }
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
        public DateTime CreationTime { get; set; }
        public static TimeSpan LifeTime => TimeSpan.FromMinutes(5);
    }
}
