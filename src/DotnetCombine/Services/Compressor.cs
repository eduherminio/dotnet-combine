using DotnetCombine.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DotnetCombine.Services
{
    public class Compressor : ICompressor
    {
        public int Run(ZipOptions options)
        {
            options.Validate();

            try
            {
                options.Input = options.Input.TrimEnd('/').TrimEnd('\\');
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

        private static List<string> FindFilesToInclude(ZipOptions options)
        {
            var dirsToExclude = options.ExcludedItems.Where(i => i.EndsWith("/") || i.EndsWith("\\"));
            var filesToExclude = options.ExcludedItems.Except(dirsToExclude);

            dirsToExclude = dirsToExclude.Select(dir => dir.TrimEnd('/').TrimEnd('\\'));

            var filesToInclude = new List<string>();
            foreach (var extension in options.Extensions)
            {
                filesToInclude.AddRange(
                    Directory.GetFiles(options.Input, $"*.{extension.TrimStart('.')}*", SearchOption.AllDirectories)
                        .Where(f =>
                            !dirsToExclude.Any(exclusion => f.IndexOf(exclusion, StringComparison.OrdinalIgnoreCase) != -1)
                            && !filesToExclude.Any(exclusion => string.Equals(Path.GetFileName(f), exclusion, StringComparison.OrdinalIgnoreCase))));
            }

            return filesToInclude;
        }

        private static void GenerateZipFile(ZipOptions options, List<string> filesToInclude)
        {
            string composeFileName(string fileNameWithoutExtension) =>
                $"{options.Prefix ?? string.Empty}" +
                $"{fileNameWithoutExtension}" +
                $"{options.Suffix ?? string.Empty}" +
                ".zip";

            string fileName = composeFileName(UniqueIdGenerator.UniqueId());
            string basePath = $"{options.Input}{Path.DirectorySeparatorChar}";

            if (options.Output is not null)
            {
                if (Path.EndsInDirectorySeparator(options.Output))
                {
                    basePath = $"{Path.TrimEndingDirectorySeparator(options.Output)}{Path.DirectorySeparatorChar}";
                    Directory.CreateDirectory(basePath);
                }
                else
                {
                    var directoryName = Path.GetDirectoryName(options.Output);

                    if (string.IsNullOrEmpty(directoryName))
                    {
                        basePath = $"{options.Input}{Path.DirectorySeparatorChar}";
                    }
                    else
                    {
                        var newDir = Directory.CreateDirectory(directoryName);
                        basePath = newDir.FullName;
                    }

                    fileName = composeFileName(Path.GetFileNameWithoutExtension(options.Output));
                }
            }

            var filePath = Path.Combine(basePath, fileName);
            var pathToTrim = $"{Path.TrimEndingDirectorySeparator(options.Input)}{Path.DirectorySeparatorChar}";

            using var fs = new FileStream(filePath, FileMode.OpenOrCreate);
            using var zip = new ZipArchive(fs, options.OverWrite ? ZipArchiveMode.Create : ZipArchiveMode.Update);
            foreach (var file in filesToInclude)
            {
                zip.CreateEntryFromFile(file, file[pathToTrim.Length..]);
            }
        }
    }
}
