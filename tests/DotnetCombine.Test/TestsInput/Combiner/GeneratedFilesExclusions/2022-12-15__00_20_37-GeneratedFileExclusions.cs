// File generated by dotnet-combine at 2022-12-15__00_20_37

using LichessTournamentAggregator.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;





// LichessTournamentAggregator\LichessTournamentAggregator\ITournamentAggregator.cs
namespace LichessTournamentAggregator
{
    public interface ITournamentAggregator
    {
        /// <summary>
        /// Aggregates the results of multiple tournaments
        /// </summary>
        /// <param name="tournamentIdsOrUrls"></param>
        /// <returns></returns>
        IAsyncEnumerable<AggregatedResult> AggregateResults(IEnumerable<string> tournamentIdsOrUrls);

        /// <summary>
        /// Aggregates the results of multiple tournaments
        /// </summary>
        /// <param name="tournamentResults"> <see cref="TournamentResult"/> </param>
        /// <returns></returns>
        IEnumerable<AggregatedResult> AggregateResults(IEnumerable<TournamentResult> tournamentResults);

        /// <summary>
        /// Aggregates the results of multiple tournaments and exports them to a CSV file
        /// </summary>
        /// <param name="tournamentIdsOrUrls"></param>
        /// <param name="fileStream">Stream where data wants to be written into</param>
        /// <param name="separator">; by default</param>
        /// <returns>.csv FileStream with aggregated results, ordered by total scores</returns>
        Task<FileStream> AggregateResultsAndExportToCsv(IEnumerable<string> tournamentIdsOrUrls, FileStream fileStream, string separator = ";");
    }
}


// LichessTournamentAggregator\LichessTournamentAggregator\TournamentAggregator.cs
namespace LichessTournamentAggregator
{
    public class TournamentAggregator : ITournamentAggregator
    {
        public async IAsyncEnumerable<AggregatedResult> AggregateResults(IEnumerable<string> tournamentIdsOrUrls)
        {
            var tournamentResults = await GetTournamentResults(tournamentIdsOrUrls).ConfigureAwait(false);

            foreach (var result in AggregateResults(tournamentResults))
            {
                yield return result;
            }
        }

        public IEnumerable<AggregatedResult> AggregateResults(IEnumerable<TournamentResult> tournamentResults)
        {
            return GroupResultsByPlayer(tournamentResults)
                .Select(grouping => new AggregatedResult(grouping));
        }

        public async Task<FileStream> AggregateResultsAndExportToCsv(IEnumerable<string> tournamentIdsOrUrls, FileStream fileStream, string separator = ";")
        {
            var orderedResults = AggregateResults(tournamentIdsOrUrls)
                .OrderByDescending(r => r.TotalScores)
                .ThenByDescending(r => r.TotalTieBreaks)
                .ThenByDescending(r => r.AveragePerformance);

            return await PopulateCsvStreamAsync(fileStream, separator, orderedResults).ConfigureAwait(false);
        }

        internal IEnumerable<Uri> GetUrls(IEnumerable<string> tournamentIdsOrUrls)
        {
            const string lichessTournamentUrl = "lichess.org/tournament/";
            const string lichessSwissUrl = "lichess.org/swiss/";
            string tournamentType = "tournament";

            foreach (var item in tournamentIdsOrUrls.Select(str => str))
            {
                var tournamentId = item.AsSpan().Trim(new char[] { ' ', '/', '#' });

                if (tournamentId.Contains(lichessTournamentUrl.AsSpan(), StringComparison.InvariantCultureIgnoreCase))
                {
                    tournamentId = tournamentId.Slice(tournamentId.LastIndexOf('/') + 1);
                }
                else if (tournamentId.Contains(lichessSwissUrl.AsSpan(), StringComparison.InvariantCultureIgnoreCase))
                {
                    tournamentId = tournamentId.Slice(tournamentId.LastIndexOf('/') + 1);
                    tournamentType = "swiss";
                }

                yield return new Uri($"https://lichess.org/api/{tournamentType}/{tournamentId.ToString()}/results");
            }
        }

        protected async Task<List<TournamentResult>> GetTournamentResults(IEnumerable<string> tournamentIdsOrUrls)
        {
            return await GetUrls(tournamentIdsOrUrls)
                .Select(GetTournamentResults)
                .Aggregate((result, next) => result.Concat(next))
                .ToListAsync()
            .ConfigureAwait(false);
        }

        private async IAsyncEnumerable<TournamentResult> GetTournamentResults(Uri url)
        {
            var client = new HttpClient();

            var response = await client.GetAsync(url).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ArgumentException("The following tournament url doesn't seem to exist",
                    url.OriginalString.Replace("/results", string.Empty));
            }

            var rawContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            foreach (var line in rawContent.Split('\n').Where(str => !string.IsNullOrWhiteSpace(str)))
            {
                yield return JsonSerializer.Deserialize<TournamentResult>(line);
            }
        }

        private static IEnumerable<IGrouping<string, TournamentResult>> GroupResultsByPlayer(IEnumerable<TournamentResult> results)
        {
            return results
                .Where(r => !string.IsNullOrWhiteSpace(r?.Username))
                .GroupBy(r => r.Username);
        }

        private static async Task<FileStream> PopulateCsvStreamAsync(FileStream fileStream, string separator, IOrderedAsyncEnumerable<AggregatedResult> aggregatedResults)
        {
            var headers = new List<string> { "#", "Username", "Total Score", "Total tie breaks", "Average Performance", "Max Rating", "Title", "Ranks", "Scores", "Tie breaks" };
            using var sw = new StreamWriter(fileStream);
            sw.WriteLine(string.Join(separator, headers));

            var internalSeparator = separator == ";" ? ", " : "; ";
            string aggregate<T>(IEnumerable<T> items) => $"[{string.Join(internalSeparator, items)}]";

            await foreach (var aggregatedResult in aggregatedResults.Select((value, i) => new { i, value }))
            {
                var result = aggregatedResult.value;
                var columns = new string[] { (aggregatedResult.i + 1).ToString(), result.Username, result.TotalScores.ToString(), result.TotalTieBreaks.ToString(), result.AveragePerformance.ToString("F"), result.MaxRating.ToString(), result.Title, aggregate(result.Ranks), aggregate(result.Scores), aggregate(result.TieBreaks) };
                sw.WriteLine(string.Join(separator, columns));
            }

            return fileStream;
        }
    }
}


// LichessTournamentAggregator\LichessTournamentAggregator.App\Program.cs
namespace LichessTournamentAggregator.App
{
    public static class Program
    {
        private const string RepoUrl = "https://github.com/eduherminio/LichessTournamentAggregator";
        private static readonly string FailureMessage = "The program has failed unexpectedly," +
            $" please have a look at our FAQ: {RepoUrl}#faqs\n" +
            "If you still experience issues, raise an issue there (or contact me)" +
            $" including the following info:{Environment.NewLine}";

        public static async Task Main(string[] args)
        {
            var tournaments = args.Length > 0
                ? args.ToList()
                : AskForTournaments().ToList();

            var aggregatedArgs = $"*\t{string.Join($"{Environment.NewLine}*\t", tournaments)}\n";
            string fileName = $"Results_{DateTime.Now.ToLocalTime():yyyy'-'MM'-'dd'__'HH'_'mm'_'ss}.csv";

            try
            {
                Console.WriteLine($"Aggregating tournaments:{Environment.NewLine}{aggregatedArgs}");

                await AggregateResultsAndCreateCsvFile(tournaments, fileName);
            }
            catch (ArgumentException e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine("Please make sure this is the tournament url you want to aggregate:");
                Console.Error.WriteLine($"*\t{e.ParamName}");
            }
            catch (HttpRequestException)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine("There may be some issues with Lichess server or you've reached the API limit.");
                Console.Error.WriteLine($"Please try again in a few minutes. If the problem persists, raise an issue in {RepoUrl}/issues");
            }
            catch (UnauthorizedAccessException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"This app doesn't have permissions to write to {Path.GetFullPath(fileName)}");
                Console.Error.WriteLine("Please run it as administrator (right click -> run it as administrator)\n" +
                    "or move the executable somewhere under C:/Users/<your user>");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(FailureMessage);
                Console.ResetColor();
                Console.Error.WriteLine("Args:\n" + aggregatedArgs
                    + "\nException: " + e.Message + Environment.NewLine + e.StackTrace);
            }
            finally
            {
                if (string.IsNullOrWhiteSpace(await File.ReadAllTextAsync(fileName)))
                {
                    File.Delete(fileName);
                }

                Console.ResetColor();
                Console.WriteLine("\nPress intro to close this window");
                Console.ReadLine();
            }
        }

        private static IEnumerable<string> AskForTournaments()
        {
            Console.WriteLine("\nType or paste the Lichess tournament ids or full tournament urls that you want to aggregate, separated by a new line.");
            Console.WriteLine("Hit enter again when finished.");

            while (true)
            {
                string arg = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(arg))
                {
                    break;
                }

                yield return arg;
            }
        }

        private static async Task AggregateResultsAndCreateCsvFile(IEnumerable<string> args, string fileName)
        {
            using FileStream fs = new FileStream(fileName, FileMode.Create);

            var aggregator = new TournamentAggregator();
            await aggregator.AggregateResultsAndExportToCsv(args, fs);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Aggregation finished, results can be found in {Path.GetFullPath(fileName)}");
        }
    }
}


// LichessTournamentAggregator\LichessTournamentAggregator.Test\AggregatedResultTest.cs
namespace LichessTournamentAggregator.Test
{
    public class AggregatedResultTest
    {
        private const string Username = "OurUser";

        [Fact]
        public void MaxRating()
        {
            ICollection<TournamentResult> results = new[]
            {
                new TournamentResult()
                {
                    Username = Username,
                    Rating = 1002
                },
                new TournamentResult()
                {
                    Username = Username,
                    Rating = 900
                },
                new TournamentResult()
                {
                    Username = Username,
                    Rating = 1300
                }
            };

            AggregatedResult aggregatedResult = new AggregatedResult(results.GroupBy(r => r.Username).Single());

            Assert.Equal(results.Max(r => r.Rating), aggregatedResult.MaxRating);
        }

        [Fact]
        public void Scores_TotalScores()
        {
            ICollection<TournamentResult> results = new[]
            {
                new TournamentResult()
                {
                    Username = Username,
                    Score = 6.5
                },
                new TournamentResult()
                {
                    Username = Username,
                    Score = 7
                },
                new TournamentResult()
                {
                    Username = Username,
                    Score = 5
                },
                new TournamentResult()
                {
                    Username = Username,
                    Score = 2
                },
                new TournamentResult()
                {
                    Username = Username,
                },
                new TournamentResult()
                {
                    Username = Guid.NewGuid().ToString(),
                    Score = 1_000
                },
            };

            AggregatedResult aggregatedResult = new AggregatedResult(results.GroupBy(r => r.Username).Single(g => g.Key == Username));

            Assert.Equal(20.5, aggregatedResult.TotalScores);
            foreach (var result in results.Where(r => r.Username == Username))
            {
                Assert.Contains(result.Score, aggregatedResult.Scores);
            }
        }

        [Fact]
        public void TotalTieBreaks()
        {
            ICollection<TournamentResult> results = new[]
            {
                new TournamentResult()
                {
                    Username = Username,
                    TieBreak = 0
                },
                new TournamentResult()
                {
                    Username = Username,
                    TieBreak = 15
                },
                new TournamentResult()
                {
                    Username = Username,
                    TieBreak = 5
                },
                new TournamentResult()
                {
                    Username = Username,
                },
                new TournamentResult()
                {
                    Username = Guid.NewGuid().ToString(),
                    TieBreak = 7
                },
            };

            AggregatedResult aggregatedResult = new AggregatedResult(results.GroupBy(r => r.Username).Single(g => g.Key == Username));

            Assert.Equal(results.Where(r => r.Username == Username).Sum(r => r.TieBreak), aggregatedResult.TotalTieBreaks);
        }

        [Fact]
        public void TieBreaks()
        {
            ICollection<TournamentResult> results = new[]
            {
                new TournamentResult()
                {
                    Username = Username,
                    TieBreak = 0
                },
                new TournamentResult()
                {
                    Username = Username,
                    TieBreak = 15
                },
                new TournamentResult()
                {
                    Username = Username,
                    TieBreak = 5
                },
                new TournamentResult()
                {
                    Username = Guid.NewGuid().ToString(),
                    TieBreak = 7
                },
            };

            AggregatedResult aggregatedResult = new AggregatedResult(results.GroupBy(r => r.Username).Single(g => g.Key == Username));

            foreach (var tieBreak in aggregatedResult.TieBreaks)
            {
                Assert.Single(results, (result) => result.Username == aggregatedResult.Username && result.TieBreak == tieBreak);
            }
        }

        [Fact]
        public void Ranks()
        {
            ICollection<TournamentResult> results = new[]
            {
                new TournamentResult()
                {
                    Username = Username,
                    Rank = 0
                },
                new TournamentResult()
                {
                    Username = Username,
                    Rank = 15
                },
                new TournamentResult()
                {
                    Username = Username,
                    Rank = 5
                },
                new TournamentResult()
                {
                    Username = Guid.NewGuid().ToString(),
                    Rank = 7
                },
            };

            AggregatedResult aggregatedResult = new AggregatedResult(results.GroupBy(r => r.Username).Single(g => g.Key == Username));

            foreach (var rank in aggregatedResult.Ranks)
            {
                Assert.Single(results, (result) => result.Username == aggregatedResult.Username && result.Rank == rank);
            }
        }

        [Fact]
        public void AveragePerformance()
        {
            ICollection<TournamentResult> results = new[]
            {
                new TournamentResult()
                {
                    Username = Username,
                    Performance = 2801,
                    Rank = 1
                },
                new TournamentResult()
                {
                    Username = Username,
                    Performance = 2401,
                    Rank = 3
                },
                new TournamentResult()
                {
                    Username = Username,
                    Performance = 2201,
                    Rank = 6
                },
                new TournamentResult()
                {
                    Username = Guid.NewGuid().ToString(),
                    Performance = 850,
                    Rank = 1601
                },
            };

            AggregatedResult aggregatedResult = new AggregatedResult(results.GroupBy(r => r.Username).Single(g => g.Key == Username));

            Assert.Equal(
                (double)results.Where(r => r.Username == Username).Sum(r => r.Performance) / results.Count(r => r.Username == Username),
                aggregatedResult.AveragePerformance);
        }

        [Fact]
        public void Title()
        {
            ICollection<TournamentResult> results = new[]
            {
                new TournamentResult()
                {
                    Username = Username,
                    Performance = 2801,
                    Rank = 2,
                    Title = "LM"
                },
                new TournamentResult()
                {
                    Username = Username,
                    Performance = 2401,
                    Rank = 2,
                    Title = "LM"
                },
                new TournamentResult()
                {
                    Username = Username,
                    Performance = 2201,
                    Rank = 3,
                    Title = "LM"
                },
                new TournamentResult()
                {
                    Username = Guid.NewGuid().ToString(),
                    Performance = 3333,
                    Rank = 1,
                    Title = "GM"
                }
            };

            AggregatedResult aggregatedResult = new AggregatedResult(results.GroupBy(r => r.Username).Single(g => g.Key == Username));

            Assert.Equal(
                results.Where(r => r.Username == Username).FirstOrDefault(r => !string.IsNullOrEmpty(r.Title))?.Title,
                aggregatedResult.Title);
        }
    }
}


// LichessTournamentAggregator\LichessTournamentAggregator.Test\TournamentAggregatorTest.cs
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


// LichessTournamentAggregator\LichessTournamentAggregator\Model\AggregatedResult.cs
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


// LichessTournamentAggregator\LichessTournamentAggregator\Model\TournamentResult.cs
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

