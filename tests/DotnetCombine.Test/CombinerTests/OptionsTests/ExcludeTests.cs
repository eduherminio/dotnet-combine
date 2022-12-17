using DotnetCombine.Options;
using DotnetCombine.Services;
using Xunit;

namespace DotnetCombine.Test.CombinerTests.OptionsTests;

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

    [Fact]
    public async Task GeneratedFileExclusions()
    {
        // Arrange
        const string inputDir = "TestsInput/Combiner/GeneratedFilesExclusions/";
        var expectedOutputFile = Path.Combine(DefaultOutputDir, nameof(GeneratedFileExclusions)) + Combiner.OutputExtension;
        var expectedOutputFileName = Path.GetFileName(expectedOutputFile);

        var pregeneratedOutputFile = Path.Combine(Path.Combine(inputDir, $"{nameof(GeneratedFileExclusions)}.cs"));
        var pregeneratedOutputFileLines = await File.ReadAllLinesAsync(pregeneratedOutputFile);

        // Act - having output file deleted
        if (File.Exists(expectedOutputFile))
        {
            File.Delete(expectedOutputFile);
        }

        var options = new CombineOptions()
        {
            ExcludedItems = Enumerable.Empty<string>(),
            Output = expectedOutputFile,
            OverWrite = true,
            Input = inputDir,
        };

        var exitCode = await new Combiner(options).Run();

        // Assert - Same content but first line
        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(expectedOutputFile));

        var generatedOutputFileContent = await File.ReadAllLinesAsync(expectedOutputFile);
        Assert.Equal(pregeneratedOutputFileLines.Length, generatedOutputFileContent.Length);

        for (int i = 1; i < pregeneratedOutputFileLines.Length; ++i)
        {
            Assert.Equal(pregeneratedOutputFileLines[i], generatedOutputFileContent[i]);
        }

        // Arrange
        File.Delete(expectedOutputFile);
        File.Copy(pregeneratedOutputFile, expectedOutputFile);

        // Act - without having output file deleted
        options = new CombineOptions()
        {
            ExcludedItems = Enumerable.Empty<string>(),
            Output = expectedOutputFile,
            OverWrite = true,
            Input = inputDir,
        };

        exitCode = await new Combiner(options).Run();

        // Assert - Same content but first line, existing output file doesn't interfere
        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(expectedOutputFile));

        generatedOutputFileContent = await File.ReadAllLinesAsync(expectedOutputFile);
        Assert.Equal(pregeneratedOutputFileLines.Length, generatedOutputFileContent.Length);

        for (int i = 1; i < pregeneratedOutputFileLines.Length; ++i)
        {
            Assert.Equal(pregeneratedOutputFileLines[i], generatedOutputFileContent[i]);
        }

        // Act - generating file with default name
        options = new CombineOptions()
        {
            ExcludedItems = new[] { expectedOutputFileName },
            Output = $"{DefaultOutputDir}/",
            OverWrite = true,
            Input = inputDir
        };

        exitCode = await new Combiner(options).Run();

        // Assert - Same content but first line, existing output file doesn't interfere
        Assert.Equal(0, exitCode);

        // Act - generating file with default name + identifyable prefix
        options = new CombineOptions()
        {
            ExcludedItems = new[] { expectedOutputFileName },
            Output = $"{DefaultOutputDir}/",
            OverWrite = true,
            Input = inputDir,
            Prefix = $"{nameof(GeneratedFileExclusions)}-"
        };

        exitCode = await new Combiner(options).Run();

        // Assert - Same content but first line, existing output file doesn't interfere
        Assert.Equal(0, exitCode);

        var prefixFile = Directory.GetFiles(DefaultOutputDir, $"{nameof(GeneratedFileExclusions)}-*").Single();
        generatedOutputFileContent = await File.ReadAllLinesAsync(prefixFile);
        Assert.Equal(pregeneratedOutputFileLines.Length, generatedOutputFileContent.Length);

        for (int i = 1; i < pregeneratedOutputFileLines.Length; ++i)
        {
            Assert.Equal(pregeneratedOutputFileLines[i], generatedOutputFileContent[i]);
        }
        // Act - generating file with default name + identifyable suffix
        options = new CombineOptions()
        {
            ExcludedItems = new[] { expectedOutputFileName },
            Output = $"{DefaultOutputDir}/",
            OverWrite = true,
            Input = inputDir,
            Suffix = $"-{nameof(GeneratedFileExclusions)}"
        };

        exitCode = await new Combiner(options).Run();

        // Assert - Same content but first line, existing output file doesn't interfere
        Assert.Equal(0, exitCode);

        var suffixFile = Directory.GetFiles(DefaultOutputDir, $"*-{nameof(GeneratedFileExclusions)}{Combiner.OutputExtension}").Single();
        generatedOutputFileContent = await File.ReadAllLinesAsync(suffixFile);
        Assert.Equal(pregeneratedOutputFileLines.Length, generatedOutputFileContent.Length);

        for (int i = 1; i < pregeneratedOutputFileLines.Length; ++i)
        {
            Assert.Equal(pregeneratedOutputFileLines[i], generatedOutputFileContent[i]);
        }
        // Act - generating file with default name + identifyable prefix + suffix
        options = new CombineOptions()
        {
            ExcludedItems = new[] { expectedOutputFileName },
            Output = $"{DefaultOutputDir}/",
            OverWrite = true,
            Input = inputDir,
            Prefix = $"{nameof(GeneratedFileExclusions)}-",
            Suffix = $"-{nameof(GeneratedFileExclusions)}"
        };

        exitCode = await new Combiner(options).Run();

        // Assert - Same content but first line, existing output file doesn't interfere
        Assert.Equal(0, exitCode);

        suffixFile = Directory.GetFiles(DefaultOutputDir, $"{nameof(GeneratedFileExclusions)}-*-{nameof(GeneratedFileExclusions)}{Combiner.OutputExtension}").Single();
        generatedOutputFileContent = await File.ReadAllLinesAsync(suffixFile);
        Assert.Equal(pregeneratedOutputFileLines.Length, generatedOutputFileContent.Length);

        for (int i = 1; i < pregeneratedOutputFileLines.Length; ++i)
        {
            Assert.Equal(pregeneratedOutputFileLines[i], generatedOutputFileContent[i]);
        }
    }
}
