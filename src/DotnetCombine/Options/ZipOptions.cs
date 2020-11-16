using System;
using CommandLine;
using System.Collections.Generic;
using System.IO;

namespace DotnetCombine.Options
{
    [Verb("zip", isDefault: false, HelpText = "Zips multiple files")]
    public class ZipOptions
    {
        [Value(0, MetaName = "input", Required = true, HelpText = @"
Input path (file or directory)")]
        public string Input { get; set; } = null!;

        [Option(shortName: 'o', longName: "output", Required = false, HelpText = @"
Output path (file or directory).
If no path is provided, the file will be created in the input directory.
If no file name is provided, a unique name based on the generation date will be used.")]
        public string? Output { get; set; }

        [Option(shortName: 'p', longName: "prefix", Required = false, HelpText = @"
Prefix for the output file.")]
        public string? Prefix { get; set; }

        [Option(shortName: 's', longName: "suffix", Required = false, HelpText = @"
Suffix for the output file.")]
        public string? Suffix { get; set; }

        [Option(longName: "exclude", Required = false, Separator = ';', Default = new[] { "bin/", "obj/" }, HelpText = @"
Excluded files and directories, separated by semicolons (;).
No regex or globalling is supported (yet), sorry!")]
        public IEnumerable<string> ExcludedItems { get; set; } = new[] { "bin/", "obj/" };

        [Option(shortName: 'f', longName: "overwrite", Required = false, Default = false, HelpText = @"
Overwrites the output file if it exists")]
        public bool OverWrite { get; set; }

        [Option(longName: "extensions", Required = false, Separator = ';', Default = new[] { ".sln", ".csproj", ".cs" }, HelpText = @"
File extensions to include")]
        public IEnumerable<string> Extensions { get; set; } = new[] { ".sln", ".csproj", ".cs" };

        public void Validate()
        {
            if (Input is null)
            {
                throw new ArgumentException($"{nameof(Input)} is required");
            }
        }
    }
}
