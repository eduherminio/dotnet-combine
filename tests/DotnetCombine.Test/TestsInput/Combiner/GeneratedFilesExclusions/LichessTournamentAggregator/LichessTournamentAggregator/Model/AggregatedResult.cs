using System;
using System.Collections.Generic;
using System.Linq;

namespace LichessTournamentAggregator.Model
{
    public class AggregatedResult
    {
        /// <summary>
        /// Lichess username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Chess title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Sum of the scores of all the tournaments
        /// </summary>
        public double TotalScores { get; set; }

        /// <summary>
        /// Sum of the Tie Breaks of all the tournaments
        /// </summary>
        public double TotalTieBreaks { get; set; }

        /// <summary>
        /// Maximum rating while playing in the tournaments
        /// </summary>
        public double MaxRating { get; set; }

        /// <summary>
        /// Rank in each tournament
        /// </summary>
        public IEnumerable<int> Ranks { get; set; }

        /// <summary>
        /// Score in each tournament
        /// </summary>
        public IEnumerable<double> Scores { get; set; }

        /// <summary>
        /// Tie breaks in each tournament
        /// </summary>
        public IEnumerable<double> TieBreaks { get; set; }

        /// <summary>
        /// Average player performance in the tournaments.
        /// </summary>
        public double AveragePerformance { get; set; }

        public AggregatedResult(IGrouping<string, TournamentResult> results)
        {
            Username = results.First().Username;
            Title = results.First().Title;
            MaxRating = results.Max(p => p.Rating);
            Ranks = results.Select(p => p.Rank);
            Scores = results.Select(p => p.Score >= p.Points ? p.Score : p.Points);  // A TournamentResult should only have either Score (Arena tournaments) or Points (Swiss tournaments)
            TieBreaks = results.Select(p => p.TieBreak);
            TotalScores = Scores.Sum();
            TotalTieBreaks = TieBreaks.Sum();
            AveragePerformance = (double)results.Select(p => p.Performance).Sum() / results.Count();
        }
    }
}
