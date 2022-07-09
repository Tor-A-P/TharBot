﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var imgurResult = ImgurResult.FromJson(jsonString);

namespace QuickType
{
    using System;
    using Newtonsoft.Json;

    public partial class ImgurResult
    {
        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("deletehash")]
        public string Deletehash { get; set; }

        [JsonProperty("account_id")]
        public object AccountId { get; set; }

        [JsonProperty("account_url")]
        public object AccountUrl { get; set; }

        [JsonProperty("ad_type")]
        public object AdType { get; set; }

        [JsonProperty("ad_url")]
        public object AdUrl { get; set; }

        [JsonProperty("title")]
        public object Title { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("width")]
        public long Width { get; set; }

        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("views")]
        public long Views { get; set; }

        [JsonProperty("section")]
        public object Section { get; set; }

        [JsonProperty("vote")]
        public object Vote { get; set; }

        [JsonProperty("bandwidth")]
        public long Bandwidth { get; set; }

        [JsonProperty("animated")]
        public bool Animated { get; set; }

        [JsonProperty("favorite")]
        public bool Favorite { get; set; }

        [JsonProperty("in_gallery")]
        public bool InGallery { get; set; }

        [JsonProperty("in_most_viral")]
        public bool InMostViral { get; set; }

        [JsonProperty("has_sound")]
        public bool HasSound { get; set; }

        [JsonProperty("is_ad")]
        public bool IsAd { get; set; }

        [JsonProperty("nsfw")]
        public object Nsfw { get; set; }

        [JsonProperty("link")]
        public Uri Link { get; set; }

        [JsonProperty("tags")]
        public object[] Tags { get; set; }

        [JsonProperty("datetime")]
        public long Datetime { get; set; }

        [JsonProperty("mp4")]
        public string Mp4 { get; set; }

        [JsonProperty("hls")]
        public string Hls { get; set; }
    }

    public partial class ImgurResult
    {
        public static ImgurResult FromJson(string json) => JsonConvert.DeserializeObject<ImgurResult>(json, QuickType.Converter.Settings);
    }
}
