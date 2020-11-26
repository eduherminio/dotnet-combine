using DotnetCombine.Options;
using DotnetCombine.Services;
using System.IO;
using System.IO.Compression;
using Xunit;

namespace DotnetCombine.Test.CompressorTests
{
    public class ExtensionsTests : BaseCompressorTests
    {
        [Theory]
        [InlineData("cs")]
        [InlineData(".cs")]
        [InlineData("*.cs")]
        public void SupportedExtensionFormats(string extension)
        {
            var outputPath = Path.Combine(DefaultOutputDir, nameof(SupportedExtensionFormats) + Compressor.OutputExtension);

            // Act
            var options = new ZipOptions()
            {
                Output = outputPath,
                OverWrite = true,
                Input = InputDir,
                Extensions = new[] { extension }
            };

            var exitCode = new Compressor(options).Run();

            // Assert
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(outputPath));

            using var fs = new FileStream(outputPath, FileMode.Open);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read);

            Assert.Equal(TotalCsFiles, zip.Entries.Count);
        }
    }
}
