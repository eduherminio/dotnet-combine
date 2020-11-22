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

        private ZipOptions _options = null!;
        private string _sanitizedInput = null!;

        public int Run(ZipOptions options)
        {
            options.Validate();

            try
            {
                _options = options;
                _sanitizedInput = _options.Input.ReplaceEndingDirectorySeparatorWithProperEndingDirectorySeparator();

                string outputFilePath = GetOutputFilePath();
                if (!_options.OverWrite && File.Exists(outputFilePath))
                {
                    throw new CombineException(
                        $"The file {outputFilePath} already exists{Environment.NewLine}" +
                        $"Did you mean to set --overwrite to true?{Environment.NewLine}" +
                        "You can also leave --output empty to always have a new one generated (and maybe use --prefix or --suffix to identify it).");
                }

                List<string> filesToInclude = FindFilesToInclude();
                GenerateZipFile(outputFilePath, filesToInclude);
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

        private string GetOutputFilePath()
        {
            string composeFileName(string fileNameWithoutExtension) =>
                (_options.Prefix ?? string.Empty) +
                fileNameWithoutExtension +
                (_options.Suffix ?? string.Empty) +
                OutputExtension;

            string fileName = composeFileName(UniqueIdGenerator.UniqueId());
            string basePath = _sanitizedInput;

            if (_options.Output is not null)
            {
                if (Path.EndsInDirectorySeparator(_options.Output))
                {
                    basePath = _options.Output.ReplaceEndingDirectorySeparatorWithProperEndingDirectorySeparator();
                    Directory.CreateDirectory(basePath);
                }
                else
                {
                    var directoryName = Path.GetDirectoryName(_options.Output);

                    basePath = string.IsNullOrEmpty(directoryName)
                        ? _options.Input + Path.DirectorySeparatorChar
                        : Directory.CreateDirectory(directoryName).FullName;

                    fileName = composeFileName(Path.GetFileNameWithoutExtension(_options.Output));
                }
            }

            return Path.Combine(basePath, fileName);
        }

        private List<string> FindFilesToInclude()
        {
            var filesToExclude = _options.ExcludedItems.Where(item => !Path.EndsInDirectorySeparator(item));

            var dirsToExclude = _options.ExcludedItems
                .Except(filesToExclude)
                .Select(dir => Path.DirectorySeparatorChar + dir.ReplaceEndingDirectorySeparatorWithProperEndingDirectorySeparator());

            var filesToInclude = new List<string>();
            foreach (var extension in _options.Extensions)
            {
                filesToInclude.AddRange(
                    Directory.GetFiles(_sanitizedInput, $"*.{extension.TrimStart('.')}", SearchOption.AllDirectories)
                        .Where(f =>
                            !dirsToExclude.Any(exclusion => $"{Path.GetDirectoryName(f)}{Path.DirectorySeparatorChar}"?.Contains(exclusion, StringComparison.OrdinalIgnoreCase) == true)
                            && !filesToExclude.Any(exclusion => string.Equals(Path.GetFileName(f), exclusion, StringComparison.OrdinalIgnoreCase))));
            }

            return filesToInclude;
        }

        private void GenerateZipFile(string zipFilePath, List<string> filesToInclude)
        {
            var pathToTrim = _sanitizedInput;

            using var fs = new FileStream(zipFilePath, _options.OverWrite ? FileMode.Create : FileMode.CreateNew);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Create);
            foreach (var file in filesToInclude)
            {
                zip.CreateEntryFromFile(file, file[pathToTrim.Length..]);
            }
        }
    }
}
