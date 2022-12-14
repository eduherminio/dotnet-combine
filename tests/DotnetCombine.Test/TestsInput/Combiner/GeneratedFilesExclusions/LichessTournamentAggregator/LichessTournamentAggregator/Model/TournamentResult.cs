using System.Text.Json.Serialization;

namespace LichessTournamentAggregator.Model
{
    public class TournamentResult
    {
        [JsonPropertyName("rank")]
        public int Rank { get; set; }

        /// <summary>
        /// Only arena
        /// </summary>
        [JsonPropertyName("score")]
        public double Score { get; set; }

        /// <summary>
        /// Only swiss
        /// </summary>
        [JsonPropertyName("points")]
        public double Points { get; set; }

        /// <summary>
        /// Only swiss
        /// </summary>
        [JsonPropertyName("tieBreak")]
        public double TieBreak { get; set; }

        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("performance")]
        public int Performance { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}
