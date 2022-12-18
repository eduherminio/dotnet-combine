using System.Linq;
using Xunit;

namespace LichessTournamentAggregator.Test
{
    public class TournamentAggregatorTest
    {
        private readonly TournamentAggregator _aggregator;

        public TournamentAggregatorTest()
        {
            _aggregator = new TournamentAggregator();
        }

        [Fact]
        public void GetUrls()
        {
            const string tournamentId = "1op8aqN0";

            var validInputs = new[]
            {
                $"https://lichess.org/tournament/{tournamentId}#",
                $"https://lichess.org/tournament/{tournamentId}/",
                $"https://lichess.org/tournament/{tournamentId}#/",
                $"https://lichess.org/tournament/{tournamentId}/#",
                $"lichess.org/tournament/{tournamentId}#",
                $"lichess.org/tournament/{tournamentId}/",
                $"lichess.org/tournament/{tournamentId}#/",
                $"lichess.org/tournament/{tournamentId}/#",
                tournamentId,
                $"{tournamentId}    ",
                $"{tournamentId}#",
                $"{tournamentId}/",
                $"{tournamentId}#/",
                $"{tournamentId}/#"
            };

            var results = _aggregator.GetUrls(validInputs).ToList();

            Assert.Equal(
                $"https://lichess.org/api/tournament/{tournamentId}/results",
                results.ToHashSet().Single().OriginalString);
        }
    }
}
