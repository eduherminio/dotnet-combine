using DotnetCombine.Model;
using DotnetCombine.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotnetCombine.Services
{
    public class Combiner
    {
        public const string OutputExtension = ".cs";

        private readonly CombineOptions _options;
        private string _outputFilePath = null!;

        public Combiner(CombineOptions options)
        {
            _options = options;
        }

        public async Task<int> Run()
        {
            try
            {
                ValidateInput();

                var filePaths = FindFilesToInclude();
                var parsedFiles = await ParseFiles(filePaths);
                await AggregateFiles(parsedFiles);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Output file: {_outputFilePath}");
                return 0;
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
                        "try running `dotnet-combine single-file` from an elevated prompt (using \"Run as Administrator\").");
                }
                else
                {
                    Console.WriteLine($"If you intended to use '{_outputFilePath}' as output file, " +
                        "try running `dotnet-combine single-file` as superuser (i.e. using 'sudo').");
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

            return Directory.GetFiles(_options.Input, $"*{OutputExtension}", SearchOption.AllDirectories)
                .Where(filePath => dirsToExclude?.Any(exclusion => $"{Path.GetDirectoryName(filePath)}{Path.DirectorySeparatorChar}"?.Contains(exclusion, StringComparison.OrdinalIgnoreCase) == true) == false
                                && filesToExclude?.Any(exclusion => string.Equals(Path.GetFileName(filePath), exclusion, StringComparison.OrdinalIgnoreCase)) == false)
                .ToList();
        }

        private async Task<ICollection<SourceFile>> ParseFiles(IEnumerable<string> filePaths)
        {
            var tasks = filePaths.Select(async filePath =>
            {
                var parsedFile = new SourceFile(filePath);
                await parsedFile.Parse(_options);
                return parsedFile;
            });

            return (await Task.WhenAll(tasks)).ToList();
        }

        private async Task AggregateFiles(ICollection<SourceFile> parsedFiles)
        {
            var includeSection = string.Concat(parsedFiles.SelectMany(p => p.Usings).Distinct().OrderBy(_ => _));

            var codeSection = new StringBuilder();

            var orderedFiles = parsedFiles.OrderBy(file => file.Namespace?.Length).ToList();     // Top level statements first

            for (int i = 0; i < parsedFiles.Count; ++i)
            {
                var parsedFile = orderedFiles[i];

                if (_options.Verbose)
                {
                    Console.WriteLine($"\t* [{i + 1}/{orderedFiles.Count}] Aggregating {parsedFile.Filepath}");
                }

                codeSection.Append(Environment.NewLine);
                codeSection.Append(string.Join(Environment.NewLine, parsedFile.Code));
                codeSection.Append(Environment.NewLine);
            }

            using var fs = new FileStream(_outputFilePath, _options.OverWrite ? FileMode.Create : FileMode.CreateNew);
            using var sw = new StreamWriter(fs);
            await sw.WriteLineAsync($"// File generated by dotnet-combine at {DateTime.Now.ToLocalTime().ToString(UniqueIdGenerator.DateFormat)}{Environment.NewLine}");
            await sw.WriteAsync(includeSection);
            await sw.WriteAsync(Environment.NewLine);
            await sw.WriteAsync(Environment.NewLine);
            await sw.WriteAsync(codeSection);
        }
    }
}
