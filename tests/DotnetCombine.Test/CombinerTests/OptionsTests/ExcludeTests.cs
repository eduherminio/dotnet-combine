using DotnetCombine.Options;
using DotnetCombine.Services;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Xunit;

namespace DotnetCombine.Test.CombinerTests.OptionsTests
{
    public class ExcludeTests : BaseCombinerTests
    {
        [Fact]
        public async Task ExcludeFile()
        {
            // Arrange
            var expectedOutputFile = Path.Combine(DefaultOutputDir, nameof(ExcludeFile)) + Combiner.OutputExtension;
            const string excludedFile = "TConverter.cs";

            // Act
            var options = new CombineOptions()
            {
                ExcludedItems = new[] { excludedFile },
                Output = expectedOutputFile,
                OverWrite = true,
                Input = InputDir,
            };

            var exitCode = await new Combiner(options).Run();

            // Assert - excluded file is not included in the zip
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(expectedOutputFile));

            var fileContent = await File.ReadAllTextAsync(expectedOutputFile);
            Assert.Contains("ParsingException", fileContent);
            Assert.Contains("interface IParsedFile", fileContent);
            Assert.Contains("interface IParsedLine", fileContent);
            Assert.DoesNotContain("class TConverter", fileContent);
        }

        [Fact]
        public async Task ExcludeDir()
        {
            // Arrange
            var expectedOutputFile = Path.Combine(DefaultOutputDir, nameof(ExcludeDir)) + Combiner.OutputExtension;
            const string excludedDir = "Interfaces/";

            // Act
            var options = new CombineOptions()
            {
                ExcludedItems = new[] { excludedDir },
                Output = expectedOutputFile,
                OverWrite = true,
                Input = InputDir,
            };

            var exitCode = await new Combiner(options).Run();

            // Assert - excluded file is not included in the zip
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(expectedOutputFile));

            var fileContent = await File.ReadAllTextAsync(expectedOutputFile);
            Assert.Contains("ParsingException", fileContent);
            Assert.Contains("class TConverter", fileContent);
            Assert.DoesNotContain("interface IParsedFile", fileContent);
            Assert.DoesNotContain("interface IParsedLine", fileContent);
        }

        [Fact]
        public async Task ExcludeFileAndDir()
        {
            // Arrange
            var expectedOutputFile = Path.Combine(DefaultOutputDir, nameof(ExcludeFileAndDir), nameof(ExcludeFileAndDir)) + Combiner.OutputExtension;
            var excludes = new[] { "TConverter.cs", "Interfaces/" };

            // Act
            var options = new CombineOptions()
            {
                ExcludedItems = excludes,
                Output = expectedOutputFile,
                OverWrite = true,
                Input = InputDir,
            };

            var exitCode = await new Combiner(options).Run();

            // Assert - excluded file is not included in the zip
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(expectedOutputFile));

            var fileContent = await File.ReadAllTextAsync(expectedOutputFile);
            Assert.Contains("ParsingException", fileContent);
            Assert.DoesNotContain("class TConverter", fileContent);
            Assert.DoesNotContain("interface IParsedFile", fileContent);
            Assert.DoesNotContain("interface IParsedLine", fileContent);
        }
    }
}
