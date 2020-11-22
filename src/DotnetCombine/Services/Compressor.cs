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

        private readonly ZipOptions _options;
        private string _outputFilePath = null!;

        public Compressor(ZipOptions options)
        {
            _options = options;
        }

        public int Run()
        {
            try
            {
                ValidateInput();

                var filesToInclude = FindFilesToInclude();
                GenerateZipFile(filesToInclude);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
#if DEBUG
                Console.WriteLine(e.GetType() + Environment.NewLine + e.StackTrace);
#endif
                Console.ForegroundColor = ConsoleColor.Yellow;
                if (OperatingSystem.IsWindows())
                {
                    Console.WriteLine($"If you intended to use '{_outputFilePath}' as output file, " +
                        $"try running `dotnet-combine zip` from an elevated prompt (using \"Run as Administrator\").");
                }
                else
                {
                    Console.WriteLine($"If you intended to use '{_outputFilePath}' as output file, " +
                        $"try running `dotnet-combine zip` as superuser (i.e. using 'sudo').");
                }

                return 1;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
#if DEBUG
                Console.WriteLine(e.GetType() + Environment.NewLine + e.StackTrace);
#endif
                return 1;
            }
            finally
            {
                Console.ResetColor();
            }

            Console.WriteLine($"Output file: {_outputFilePath}");
            return 0;
        }

        private void ValidateInput()
        {
            _options.Validate();

            _outputFilePath = GetOutputFilePath();
            if (!_options.OverWrite && File.Exists(_outputFilePath))
            {
                throw new CombineException(
                    $"The file {_outputFilePath} already exists{Environment.NewLine}" +
                    $"Did you mean to set --overwrite to true?{Environment.NewLine}" +
                    "You can also leave --output empty to always have a new one generated (and maybe use --prefix or --suffix to identify it).");
            }
        }

        private string GetOutputFilePath()
        {
            string composeFileName(string fileNameWithoutExtension) =>
                (_options.Prefix ?? string.Empty) +
                fileNameWithoutExtension +
                (_options.Suffix ?? string.Empty) +
                OutputExtension;

            string fileName = composeFileName(UniqueIdGenerator.UniqueId());
            string basePath = File.Exists(_options.Input)
                 ? Path.GetDirectoryName(_options.Input) ?? throw new CombineException($"{_options.Input} parent dir not found, try providing an absolute or relative path")
                 : _options.Input!;

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

        private ICollection<string> FindFilesToInclude()
        {
            if (File.Exists(_options.Input))
            {
                return new List<string> { _options.Input };
            }

            var filesToExclude = _options.ExcludedItems.Where(item => !Path.EndsInDirectorySeparator(item));

            var dirsToExclude = _options.ExcludedItems
                .Except(filesToExclude)
                .Select(dir => Path.DirectorySeparatorChar + dir.ReplaceEndingDirectorySeparatorWithProperEndingDirectorySeparator());

            var filesToInclude = new List<string>();
            foreach (var extension in _options.Extensions)
            {
                filesToInclude.AddRange(
                    Directory.GetFiles(_options.Input, $"*.{extension.TrimStart('.')}", SearchOption.AllDirectories)
                        .Where(f => !dirsToExclude.Any(exclusion => $"{Path.GetDirectoryName(f)}{Path.DirectorySeparatorChar}"?.Contains(exclusion, StringComparison.OrdinalIgnoreCase) == true)
                                && !filesToExclude.Any(exclusion => string.Equals(Path.GetFileName(f), exclusion, StringComparison.OrdinalIgnoreCase))));
            }

            return filesToInclude;
        }

        private void GenerateZipFile(IEnumerable<string> filesToInclude)
        {
            var pathToTrim = Directory.Exists(_options.Input)
                ? _options.Input.ReplaceEndingDirectorySeparatorWithProperEndingDirectorySeparator()
                : string.Empty;

            using var fs = new FileStream(_outputFilePath, _options.OverWrite ? FileMode.Create : FileMode.CreateNew);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Create);
            foreach (var file in filesToInclude)
            {
                zip.CreateEntryFromFile(file, file[pathToTrim.Length..]);
            }
        }
    }
}
