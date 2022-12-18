using LichessTournamentAggregator.Model;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
