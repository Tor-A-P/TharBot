﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using QuickType;
//
//    var animeResult = AnimeResult.FromJson(jsonString);

namespace QuickType
{
    using System;
    using Newtonsoft.Json;

    public partial class AnimeResult
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("main_picture")]
        public Picture MainPicture { get; set; }

        [JsonProperty("alternative_titles")]
        public AlternativeTitles AlternativeTitles { get; set; }

        [JsonProperty("start_date")]
        public DateTime StartDate { get; set; }

        [JsonProperty("end_date")]
        public DateTime EndDate { get; set; }

        [JsonProperty("synopsis")]
        public string Synopsis { get; set; }

        [JsonProperty("mean")]
        public double Mean { get; set; }

        [JsonProperty("rank")]
        public long Rank { get; set; }

        [JsonProperty("popularity")]
        public long Popularity { get; set; }

        [JsonProperty("num_list_users")]
        public long NumListUsers { get; set; }

        [JsonProperty("num_scoring_users")]
        public long NumScoringUsers { get; set; }

        [JsonProperty("nsfw")]
        public string Nsfw { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("media_type")]
        public string MediaType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("genres")]
        public Genre[] Genres { get; set; }

        [JsonProperty("my_list_status")]
        public MyListStatus MyListStatus { get; set; }

        [JsonProperty("num_episodes")]
        public long NumEpisodes { get; set; }

        [JsonProperty("start_season")]
        public StartSeason StartSeason { get; set; }

        [JsonProperty("broadcast")]
        public Broadcast Broadcast { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("average_episode_duration")]
        public long AverageEpisodeDuration { get; set; }

        [JsonProperty("rating")]
        public string Rating { get; set; }

        [JsonProperty("pictures")]
        public Picture[] Pictures { get; set; }

        [JsonProperty("background")]
        public string Background { get; set; }

        [JsonProperty("related_anime")]
        public RelatedAnime[] RelatedAnime { get; set; }

        [JsonProperty("related_manga")]
        public object[] RelatedManga { get; set; }

        [JsonProperty("recommendations")]
        public Recommendation[] Recommendations { get; set; }

        [JsonProperty("studios")]
        public Genre[] Studios { get; set; }

        [JsonProperty("statistics")]
        public Statistics Statistics { get; set; }
    }

    public partial class AlternativeTitles
    {
        [JsonProperty("synonyms")]
        public string[] Synonyms { get; set; }

        [JsonProperty("en")]
        public string En { get; set; }

        [JsonProperty("ja")]
        public string Ja { get; set; }
    }

    public partial class Broadcast
    {
        [JsonProperty("day_of_the_week")]
        public string DayOfTheWeek { get; set; }

        [JsonProperty("start_time")]
        public string StartTime { get; set; }
    }

    public partial class Genre
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial class Picture
    {
        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }
    }

    public partial class MyListStatus
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("score")]
        public long Score { get; set; }

        [JsonProperty("num_episodes_watched")]
        public long NumEpisodesWatched { get; set; }

        [JsonProperty("is_rewatching")]
        public bool IsRewatching { get; set; }

        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
    }

    public partial class Recommendation
    {
        [JsonProperty("node")]
        public Node Node { get; set; }

        [JsonProperty("num_recommendations")]
        public long NumRecommendations { get; set; }
    }

    public partial class Node
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("main_picture")]
        public Picture MainPicture { get; set; }
    }

    public partial class RelatedAnime
    {
        [JsonProperty("node")]
        public Node Node { get; set; }

        [JsonProperty("relation_type")]
        public string RelationType { get; set; }

        [JsonProperty("relation_type_formatted")]
        public string RelationTypeFormatted { get; set; }
    }

    public partial class StartSeason
    {
        [JsonProperty("year")]
        public long Year { get; set; }

        [JsonProperty("season")]
        public string Season { get; set; }
    }

    public partial class Statistics
    {
        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("num_list_users")]
        public long NumListUsers { get; set; }
    }

    public partial class Status
    {
        [JsonProperty("watching")]
        public long Watching { get; set; }

        [JsonProperty("completed")]
        public long Completed { get; set; }

        [JsonProperty("on_hold")]
        public long OnHold { get; set; }

        [JsonProperty("dropped")]
        public long Dropped { get; set; }

        [JsonProperty("plan_to_watch")]
        public long PlanToWatch { get; set; }
    }

    public partial class AnimeResult
    {
        public static AnimeResult FromJson(string json) => JsonConvert.DeserializeObject<AnimeResult>(json, Converter.Settings);
    }
}
