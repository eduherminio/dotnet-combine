using DotnetCombine.Options;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;

namespace DotnetCombine.Test.CompressorTests
{
    public class OutputTests : BaseCompressorTests
    {
        private static DateTime? ParseDateTimeFromFileName(string fileName)
        {
            return DateTime.TryParseExact(fileName, UniqueIdGenerator.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var date)
                ? (DateTime?)date
                : null;
        }

        [Theory]
        [InlineData("OutPutfilename")]
        [InlineData("OutPutfilename.rar")]
        [InlineData("OutPutfilename.zip")]
        [InlineData(DefaultOutputDir + "/OutPutfilename")]
        [InlineData(DefaultOutputDir + "/nonexistingFolder/OutPutfilename")]
        [InlineData(DefaultOutputDir + "/nonexistingFolder/OutPutfilename.zip")]
        public void OutPut(string output)
        {
            // Act
            var options = new ZipOptions()
            {
                Output = output,
                OverWrite = true,
                Input = InputDir
            };

            var exitCode = _compressor.Run(options);

            // Assert
            Assert.Equal(0, exitCode);
            var path = string.IsNullOrEmpty(Path.GetDirectoryName(output))
                ? InputDir
                : Path.GetDirectoryName(output)!;
            var existingFiles = Directory.GetFiles(path);

            var zipFiles = existingFiles.Where(f => Path.GetFileNameWithoutExtension(f) == "OutPutfilename" && Path.GetExtension(f) == ".zip");

            Assert.Single(zipFiles);
        }

        [Theory]
        [InlineData(DefaultOutputDir + "/")]
        [InlineData(DefaultOutputDir + "//")]
        public void NoOutputFileName_GeneratesUniqueFileName(string outputDir)
        {
            // Arrange
            var timeBefore = ParseDateTimeFromFileName(UniqueIdGenerator.UniqueId());

            // Act
            var options = new ZipOptions()
            {
                Output = outputDir,
                OverWrite = true,
                Input = InputDir
            };

            var exitCode = _compressor.Run(options);

            // Assert
            Assert.Equal(0, exitCode);
            var existingFiles = Directory.GetFiles(outputDir);

            var timeAfter = ParseDateTimeFromFileName(UniqueIdGenerator.UniqueId());
            var zipFiles = existingFiles.Where(f =>
            {
                if (Path.GetExtension(f) == ".zip")
                {
                    var fileDate = ParseDateTimeFromFileName(Path.GetFileNameWithoutExtension(f));
                    return fileDate is not null && fileDate >= timeBefore && fileDate <= timeAfter;
                }

                return false;
            });

            Assert.Single(zipFiles);
        }

        [Theory]
        [InlineData(nameof(NoOutputDir_UsesInputDirAndOutputFileName), "")]
        [InlineData(nameof(NoOutputDir_UsesInputDirAndOutputFileName), ".zip")]
        public void NoOutputDir_UsesInputDirAndOutputFileName(string fileName, string extension)
        {
            // Act
            var options = new ZipOptions()
            {
                Output = fileName + extension,
                OverWrite = true,
                Input = InputDir
            };

            var exitCode = _compressor.Run(options);

            // Assert
            Assert.Equal(0, exitCode);
            var existingFiles = Directory.GetFiles(InputDir);

            var zipFiles = existingFiles.Where(f => Path.GetFileNameWithoutExtension(f) == fileName && Path.GetExtension(f) == ".zip");

            Assert.Single(zipFiles);
        }

        [Fact]
        public void NoOutput_UsesInputDirAndGeneratesUniqueFileName()
        {
            // Arrange
            var timeBefore = ParseDateTimeFromFileName(UniqueIdGenerator.UniqueId());

            // Act
            var options = new ZipOptions()
            {
                Output = null,
                Input = InputDir,
                OverWrite = true,
            };

            var exitCode = _compressor.Run(options);

            // Assert
            Assert.Equal(0, exitCode);
            var existingFiles = Directory.GetFiles(InputDir);

            var timeAfter = ParseDateTimeFromFileName(UniqueIdGenerator.UniqueId());
            var zipFiles = existingFiles.Where(f =>
            {
                if (Path.GetExtension(f) == ".zip")
                {
                    var fileDate = ParseDateTimeFromFileName(Path.GetFileNameWithoutExtension(f));
                    return fileDate is not null && fileDate >= timeBefore && fileDate <= timeAfter;
                }

                return false;
            });

            Assert.Single(zipFiles);
        }

        [Theory]
        [InlineData(DefaultOutputDir + "\\OutputPrefix\\")]
        [InlineData(DefaultOutputDir + "\\OutputPrefix\\filename")]
        [InlineData(DefaultOutputDir + "\\OutputPrefix\\filename.zip")]
        public void OutputPrefix(string output)
        {
            // Arrange
            var prefix = $"prefix-{output.Replace("\\", "-")}-";

            // Act
            var options = new ZipOptions()
            {
                Prefix = prefix,
                Output = output,
                Input = InputDir
            };

            var exitCode = _compressor.Run(options);

            // Assert
            Assert.Equal(0, exitCode);
            var path = string.IsNullOrEmpty(Path.GetDirectoryName(output))
                ? InputDir
                : Path.GetDirectoryName(output)!;
            var existingFiles = Directory.GetFiles(path);

            var zipFiles = existingFiles.Where(f =>
                Path.GetFileNameWithoutExtension(f).StartsWith(prefix)
                && Path.GetExtension(f) == ".zip");

            Assert.Single(zipFiles);
        }

        [Theory]
        [InlineData(DefaultOutputDir + "\\OutputSuffix\\")]
        [InlineData(DefaultOutputDir + "\\OutputSuffix\\filename")]
        [InlineData(DefaultOutputDir + "\\OutputSuffix\\filename.zip")]
        public void OutputSuffix(string output)
        {
            // Arrange
            var suffix = $"-{output.Replace("\\", "-")}-suffix";

            // Act
            var options = new ZipOptions()
            {
                Suffix = suffix,
                Output = output,
                OverWrite = true,
                Input = InputDir
            };

            var exitCode = _compressor.Run(options);

            // Assert
            Assert.Equal(0, exitCode);
            var path = string.IsNullOrEmpty(Path.GetDirectoryName(output))
                ? InputDir
                : Path.GetDirectoryName(output)!;
            var existingFiles = Directory.GetFiles(path);

            var zipFiles = existingFiles.Where(f =>
                Path.GetFileNameWithoutExtension(f).EndsWith(suffix)
                && Path.GetExtension(f) == ".zip");

            Assert.Single(zipFiles);
        }

        [Theory]
        [InlineData(DefaultOutputDir + "\\OutputPrefixSuffix\\")]
        [InlineData(DefaultOutputDir + "\\OutputPrefixSuffix\\filename")]
        [InlineData(DefaultOutputDir + "\\OutputPrefixSuffix\\filename.zip")]
        public void OutputPrefixSuffix(string output)
        {
            // Arrange
            var prefix = $"preprefix-{output.Replace("\\", "-")}-";
            var suffix = $"-{output.Replace("\\", "-")}-suffixfix";

            // Act
            var options = new ZipOptions()
            {
                Prefix = prefix,
                Suffix = suffix,
                Output = output,
                OverWrite = true,
                Input = InputDir
            };

            var exitCode = _compressor.Run(options);

            // Assert
            Assert.Equal(0, exitCode);
            var path = string.IsNullOrEmpty(Path.GetDirectoryName(output))
                ? InputDir
                : Path.GetDirectoryName(output)!;
            var existingFiles = Directory.GetFiles(path);

            var zipFiles = existingFiles.Where(f =>
                Path.GetFileNameWithoutExtension(f).StartsWith(prefix)
                && Path.GetFileNameWithoutExtension(f).EndsWith(suffix)
                && Path.GetExtension(f) == ".zip");

            Assert.Single(zipFiles);
        }
    }
}
