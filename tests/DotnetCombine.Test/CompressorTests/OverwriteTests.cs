using DotnetCombine.Options;
using DotnetCombine.Services;
using SheepTools;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace DotnetCombine.Test.CompressorTests
{
    public class OverwriteTests : BaseCompressorTests
    {
        [Fact]
        public void OverWrite_CreatesANewFile()
        {
            // Arrange - create a pre-existing .zip file with one .cs file inside
            var initialCsFile = Path.Combine(InputDir, "cs1.cs");
            var expectedZipFile = Path.Combine(DefaultOutputDir, nameof(OverWrite_CreatesANewFile)) + Compressor.OutputExtension;

            Ensure.True(File.Exists(initialCsFile));
            CreateZipFile(expectedZipFile, initialCsFile);

            // Act
            var options = new ZipOptions()
            {
                OverWrite = true,
                Input = InputDir,
                Output = expectedZipFile,
                Extensions = new[] { ".txt" }
            };

            var exitCode = _compressor.Run(options);

            // Assert - final file doesn't include original file's content
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(expectedZipFile));

            using var fs = new FileStream(expectedZipFile, FileMode.Open);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read);

            Assert.Equal(TotalTxtFiles, zip.Entries.Count);
            foreach (var file in zip.Entries)
            {
                Assert.EndsWith(".txt", file.Name);
            }
        }

        [Fact]
        public void NoOverwrite_UpdatesExistingFile()
        {
            // Arrange - create a pre-existing .zip file with one .cs file inside
            var initialCsFile = Path.Combine(InputDir, "cs1.cs");
            var expectedZipFile = Path.Combine(DefaultOutputDir, nameof(NoOverwrite_UpdatesExistingFile)) + Compressor.OutputExtension;

            Ensure.True(File.Exists(initialCsFile));
            CreateZipFile(expectedZipFile, initialCsFile);
            Ensure.True(File.Exists(expectedZipFile));

            // Act
            var options = new ZipOptions()
            {
                OverWrite = false,
                Input = InputDir,
                Output = expectedZipFile,
                Extensions = new[] { ".txt" }
            };

            var exitCode = _compressor.Run(options);

            // Assert - final file includes original file's content
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(expectedZipFile));

            using var fs = new FileStream(expectedZipFile, FileMode.Open);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read);

            Assert.Equal(TotalTxtFiles + 1, zip.Entries.Count);
            Assert.Equal(TotalTxtFiles, zip.Entries.Count(e => e.Name.EndsWith(".txt")));
            Assert.Equal(1, zip.Entries.Count(e => e.Name.EndsWith(".cs")));
        }

        private static void CreateZipFile(string filePath, params string[] filesToInclude)
        {
            string pathToTrim = $"{InputDir}{Path.DirectorySeparatorChar}";

            using var fs = new FileStream(filePath, FileMode.OpenOrCreate);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Create);
            foreach (var file in filesToInclude)
            {
                zip.CreateEntryFromFile(file, file[pathToTrim.Length..]);
            }
        }
    }
}
