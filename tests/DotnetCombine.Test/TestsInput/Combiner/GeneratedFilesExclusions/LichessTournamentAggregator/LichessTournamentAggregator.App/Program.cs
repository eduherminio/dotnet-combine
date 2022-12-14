using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

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
