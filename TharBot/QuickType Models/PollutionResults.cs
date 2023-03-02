﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var pollution = Pollution.FromJson(jsonString);

namespace QuickType
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public partial class PollutionResult
    {
        [JsonProperty("coord")]
        public JsonArrayAttribute Coord { get; set; }

        [JsonProperty("list")]
        public List[] List { get; set; }
    }

    public partial class List
    {
        [JsonProperty("dt")]
        public long Dt { get; set; }

        [JsonProperty("main")]
        public Main Main { get; set; }

        [JsonProperty("components")]
        public Dictionary<string, double> Components { get; set; }
    }

    public partial class Main
    {
        [JsonProperty("aqi")]
        public long Aqi { get; set; }
    }

    public partial class PollutionResult
    {
        public static PollutionResult FromJson(string json) => JsonConvert.DeserializeObject<PollutionResult>(json, Converter.Settings);
    }

}