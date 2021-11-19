using DotnetCombine.Options;
using DotnetCombine.Services;
using System.Globalization;
using Xunit;

namespace DotnetCombine.Test.CompressorTests;

public class OutputTests : BaseCompressorTests
{
    [Theory]
    [InlineData("OutPutfilename")]
    [InlineData("OutPutfilename.rar")]
    [InlineData("OutPutfilename" + Compressor.OutputExtension)]
    [InlineData(DefaultOutputDir + "/OutPutfilename")]
    [InlineData(DefaultOutputDir + "/nonexistingFolder/OutPutfilename")]
    [InlineData(DefaultOutputDir + "/nonexistingFolder/OutPutfilename" + Compressor.OutputExtension)]
    public void OutPut(string output)
    {
        // Act
        var options = new ZipOptions()
        {
            Output = output,
            OverWrite = true,
            Input = InputDir
        };

        var exitCode = new Compressor(options).Run();

        // Assert
        Assert.Equal(0, exitCode);
        var path = string.IsNullOrEmpty(Path.GetDirectoryName(output))
            ? InputDir
            : Path.GetDirectoryName(output)!;
        var existingFiles = Directory.GetFiles(path);

        var zipFiles = existingFiles.Where(f => Path.GetFileNameWithoutExtension(f) == "OutPutfilename" && Path.GetExtension(f) == Compressor.OutputExtension);

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

        var exitCode = new Compressor(options).Run();

        // Assert
        Assert.Equal(0, exitCode);
        var existingFiles = Directory.GetFiles(outputDir);

        var timeAfter = ParseDateTimeFromFileName(UniqueIdGenerator.UniqueId());
        var zipFiles = existingFiles.Where(f =>
        {
            if (Path.GetExtension(f) == Compressor.OutputExtension)
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
    [InlineData(nameof(NoOutputDir_UsesInputDirAndOutputFileName), Compressor.OutputExtension)]
    public void NoOutputDir_UsesInputDirAndOutputFileName(string fileName, string extension)
    {
        // Act
        var options = new ZipOptions()
        {
            Output = fileName + extension,
            OverWrite = true,
            Input = InputDir
        };

        var exitCode = new Compressor(options).Run();

        // Assert
        Assert.Equal(0, exitCode);
        var existingFiles = Directory.GetFiles(InputDir);

        var zipFiles = existingFiles.Where(f => Path.GetFileNameWithoutExtension(f) == fileName && Path.GetExtension(f) == Compressor.OutputExtension);

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

        var exitCode = new Compressor(options).Run();

        // Assert
        Assert.Equal(0, exitCode);
        var existingFiles = Directory.GetFiles(InputDir);

        var timeAfter = ParseDateTimeFromFileName(UniqueIdGenerator.UniqueId());
        var zipFiles = existingFiles.Where(f =>
        {
            if (Path.GetExtension(f) == Compressor.OutputExtension)
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
    [InlineData(DefaultOutputDir + "\\OutputPrefix\\filename" + Compressor.OutputExtension)]
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

        var exitCode = new Compressor(options).Run();

        // Assert
        Assert.Equal(0, exitCode);
        var path = string.IsNullOrEmpty(Path.GetDirectoryName(output))
            ? InputDir
            : Path.GetDirectoryName(output)!;
        var existingFiles = Directory.GetFiles(path);

        var zipFiles = existingFiles.Where(f =>
            Path.GetFileNameWithoutExtension(f).StartsWith(prefix)
            && Path.GetExtension(f) == Compressor.OutputExtension);

        Assert.Single(zipFiles);
    }

    [Theory]
    [InlineData(DefaultOutputDir + "\\OutputSuffix\\")]
    [InlineData(DefaultOutputDir + "\\OutputSuffix\\filename")]
    [InlineData(DefaultOutputDir + "\\OutputSuffix\\filename" + Compressor.OutputExtension)]
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

        var exitCode = new Compressor(options).Run();

        // Assert
        Assert.Equal(0, exitCode);
        var path = string.IsNullOrEmpty(Path.GetDirectoryName(output))
            ? InputDir
            : Path.GetDirectoryName(output)!;
        var existingFiles = Directory.GetFiles(path);

        var zipFiles = existingFiles.Where(f =>
            Path.GetFileNameWithoutExtension(f).EndsWith(suffix)
            && Path.GetExtension(f) == Compressor.OutputExtension);

        Assert.Single(zipFiles);
    }

    [Theory]
    [InlineData(DefaultOutputDir + "\\OutputPrefixSuffix\\")]
    [InlineData(DefaultOutputDir + "\\OutputPrefixSuffix\\filename")]
    [InlineData(DefaultOutputDir + "\\OutputPrefixSuffix\\filename" + Compressor.OutputExtension)]
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

        var exitCode = new Compressor(options).Run();

        // Assert
        Assert.Equal(0, exitCode);
        var path = string.IsNullOrEmpty(Path.GetDirectoryName(output))
            ? InputDir
            : Path.GetDirectoryName(output)!;
        var existingFiles = Directory.GetFiles(path);

        var zipFiles = existingFiles.Where(f =>
            Path.GetFileNameWithoutExtension(f).StartsWith(prefix)
            && Path.GetFileNameWithoutExtension(f).EndsWith(suffix)
            && Path.GetExtension(f) == Compressor.OutputExtension);

        Assert.Single(zipFiles);
    }

    [Fact]
    public void NoOutputAndInputFile()
    {
        // Arrange
        var options = new ZipOptions()
        {
            Input = $"{InputDir}/cs1.cs",
            Prefix = nameof(NoOutputAndInputFile),
            OverWrite = true,
        };

        // Act
        var exitCode = new Compressor(options).Run();

        // Assert
        Assert.Equal(0, exitCode);

        var existingFiles = Directory.GetFiles(InputDir);

        var zipFiles = existingFiles.Where(f => Path.GetExtension(f) == Compressor.OutputExtension
                                            && Path.GetFileNameWithoutExtension(f).Contains(options.Prefix));

        Assert.NotEmpty(zipFiles);
    }

    private static DateTime? ParseDateTimeFromFileName(string fileName)
    {
        return DateTime.TryParseExact(fileName, UniqueIdGenerator.DateFormat, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out var date)
            ? (DateTime?)date
            : null;
    }
}
