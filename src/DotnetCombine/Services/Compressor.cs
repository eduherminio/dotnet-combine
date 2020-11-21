using DotnetCombine.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DotnetCombine.Services
{
    public class Compressor
    {
        public const string OutputExtension = ".zip";
        private string _sanitizedInput = null!;

        public int Run(ZipOptions options)
        {
            options.Validate();

            try
            {
                _sanitizedInput = Path.TrimEndingDirectorySeparator(options.Input);
                List<string> filesToInclude = FindFilesToInclude(options);

                GenerateZipFile(options, filesToInclude);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                return 1;
            }
            finally
            {
                Console.ResetColor();
            }

            return 0;
        }

        private List<string> FindFilesToInclude(ZipOptions options)
        {
            var filesToExclude = options.ExcludedItems.Where(item => !Path.EndsInDirectorySeparator(item));

            var dirsToExclude = options.ExcludedItems
                .Except(filesToExclude)
                .Select(dir => Path.DirectorySeparatorChar + dir.ReplaceEndingDirectorySeparatorWithProperEndingDirectorySeparator());

            var filesToInclude = new List<string>();
            foreach (var extension in options.Extensions)
            {
                filesToInclude.AddRange(
                    Directory.GetFiles(_sanitizedInput, $"*.{extension.TrimStart('.')}*", SearchOption.AllDirectories)
                        .Where(f =>
                            !dirsToExclude.Any(exclusion => $"{Path.GetDirectoryName(f)}{Path.DirectorySeparatorChar}"?.Contains(exclusion, StringComparison.OrdinalIgnoreCase) == true)
                            && !filesToExclude.Any(exclusion => string.Equals(Path.GetFileName(f), exclusion, StringComparison.OrdinalIgnoreCase))));
            }

            return filesToInclude;
        }

        private void GenerateZipFile(ZipOptions options, List<string> filesToInclude)
        {
            string composeFileName(string fileNameWithoutExtension) =>
                (options.Prefix ?? string.Empty) +
                fileNameWithoutExtension +
                (options.Suffix ?? string.Empty) +
                OutputExtension;

            string fileName = composeFileName(UniqueIdGenerator.UniqueId());
            string basePath = _sanitizedInput;

            if (options.Output is not null)
            {
                if (Path.EndsInDirectorySeparator(options.Output))
                {
                    basePath = options.Output.ReplaceEndingDirectorySeparatorWithProperEndingDirectorySeparator();
                    Directory.CreateDirectory(basePath);
                }
                else
                {
                    var directoryName = Path.GetDirectoryName(options.Output);

                    basePath = string.IsNullOrEmpty(directoryName)
                        ? options.Input + Path.DirectorySeparatorChar
                        : Directory.CreateDirectory(directoryName).FullName;

                    fileName = composeFileName(Path.GetFileNameWithoutExtension(options.Output));
                }
            }

            var filePath = Path.Combine(basePath, fileName);
            var pathToTrim = _sanitizedInput;

            using var fs = new FileStream(filePath, FileMode.OpenOrCreate);
            using var zip = new ZipArchive(fs, options.OverWrite ? ZipArchiveMode.Create : ZipArchiveMode.Update);
            foreach (var file in filesToInclude)
            {
                zip.CreateEntryFromFile(file, file[pathToTrim.Length..]);
            }
        }
    }
}
