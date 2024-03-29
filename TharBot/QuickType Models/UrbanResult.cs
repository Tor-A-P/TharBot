﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var urban = Urban.FromJson(jsonString);

namespace QuickType
{
    using System;
    using Newtonsoft.Json;

    public partial class UrbanResult
    {
        [JsonProperty("list")]
        public List[] List { get; set; }
    }

    public partial class List
    {
        [JsonProperty("definition")]
        public string Definition { get; set; }

        [JsonProperty("permalink")]
        public Uri Permalink { get; set; }

        [JsonProperty("thumbs_up")]
        public long ThumbsUp { get; set; }

        [JsonProperty("sound_urls")]
        public Uri[] SoundUrls { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("word")]
        public string Word { get; set; }

        [JsonProperty("defid")]
        public long Defid { get; set; }

        [JsonProperty("current_vote")]
        public string CurrentVote { get; set; }

        [JsonProperty("written_on")]
        public DateTimeOffset WrittenOn { get; set; }

        [JsonProperty("example")]
        public string Example { get; set; }

        [JsonProperty("thumbs_down")]
        public long ThumbsDown { get; set; }
    }

    public partial class UrbanResult
    {
        public static UrbanResult FromJson(string json) => JsonConvert.DeserializeObject<UrbanResult>(json, Converter.Settings);
    }
}
