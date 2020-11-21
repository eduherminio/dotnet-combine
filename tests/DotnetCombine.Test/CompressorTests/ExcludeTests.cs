using DotnetCombine.Options;
using DotnetCombine.Services;
using System;
using System.IO;
using System.IO.Compression;
using Xunit;

namespace DotnetCombine.Test.CompressorTests
{
    public class ExcludeTests : BaseCompressorTests
    {
        [Fact]
        public void ExcludeFile()
        {
            // Arrange
            var expectedZipFile = Path.Combine(DefaultOutputDir, nameof(ExcludeFile)) + Compressor.OutputExtension;
            const string excludedFile = "cs1.cs";

            // Act
            var options = new ZipOptions()
            {
                ExcludedItems = new[] { excludedFile },
                Extensions = new[] { ".cs", ".txt" },
                Output = expectedZipFile,
                OverWrite = true,
                Input = InputDir,
            };

            var exitCode = _compressor.Run(options);

            // Assert - excluded file is not included in the zip
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(expectedZipFile));

            using var fs = new FileStream(expectedZipFile, FileMode.Open);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read);

            Assert.Equal(TotalCsFiles + TotalTxtFiles - 1, zip.Entries.Count);
            Assert.DoesNotContain(zip.Entries, e => Path.GetFileName(e.Name).Equals(excludedFile, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void ExcludeDir()
        {
            // Arrange
            var expectedZipFile = Path.Combine(DefaultOutputDir, nameof(ExcludeDir)) + Compressor.OutputExtension;
            const string excludedDir = "dir1/";

            // Act
            var options = new ZipOptions()
            {
                ExcludedItems = new[] { excludedDir },
                Extensions = new[] { ".cs", ".txt" },
                Output = expectedZipFile,
                OverWrite = true,
                Input = InputDir,
            };

            var exitCode = _compressor.Run(options);

            // Assert - excluded file is not included in the zip
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(expectedZipFile));

            using var fs = new FileStream(expectedZipFile, FileMode.Open);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read);

            Assert.Equal(TotalCsFiles + TotalTxtFiles - 2, zip.Entries.Count);
            Assert.DoesNotContain(zip.Entries, e => Path.GetDirectoryName(e.Name)!.Contains(Path.TrimEndingDirectorySeparator(excludedDir), StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void ExcludeFileAndDir()
        {
            // Arrange
            var expectedZipFile = Path.Combine(DefaultOutputDir, nameof(ExcludeFileAndDir), nameof(ExcludeFileAndDir)) + Compressor.OutputExtension;
            var excludes = new[] { "cs1.cs", "dir1/" };

            // Act
            var options = new ZipOptions()
            {
                ExcludedItems = excludes,
                Extensions = new[] { ".cs", ".txt" },
                Output = expectedZipFile,
                OverWrite = true,
                Input = InputDir,
            };

            var exitCode = _compressor.Run(options);

            // Assert - excluded file is not included in the zip
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(expectedZipFile));

            using var fs = new FileStream(expectedZipFile, FileMode.Open);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read);

            Assert.Equal(TotalCsFiles + TotalTxtFiles - 3, zip.Entries.Count);
            foreach (var excludeRule in excludes)
            {
                if (Path.EndsInDirectorySeparator(excludeRule))
                {
                    Assert.DoesNotContain(zip.Entries, e => Path.GetDirectoryName(e.Name)!.Contains(Path.TrimEndingDirectorySeparator(excludeRule), StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    Assert.DoesNotContain(zip.Entries, e => Path.GetFileName(e.Name).Equals(excludeRule, StringComparison.OrdinalIgnoreCase));
                }
            }
        }
    }
}
