﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var animeListResult = AnimeListResult.FromJson(jsonString);

namespace QuickType
{
    using System;
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class AnimeListResult
    {
        [JsonProperty("data")]
        public Datum[] Data { get; set; }

        [JsonProperty("paging")]
        public Paging Paging { get; set; }
    }

    public partial class Datum
    {
        [JsonProperty("node")]
        public ALNode Node { get; set; }
    }

    public partial class ALNode
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("main_picture")]
        public MainPicture MainPicture { get; set; }
    }

    public partial class MainPicture
    {
        [JsonProperty("medium")]
        public Uri Medium { get; set; }

        [JsonProperty("large")]
        public Uri Large { get; set; }
    }

    public partial class Paging
    {
        [JsonProperty("next")]
        public Uri Next { get; set; }
    }

    public partial class AnimeListResult
    {
        public static AnimeListResult FromJson(string json) => JsonConvert.DeserializeObject<AnimeListResult>(json, Converter.Settings);
    }
}
