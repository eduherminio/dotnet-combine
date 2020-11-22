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
            // Arrange - create a pre-existing 'output' file
            var expectedZipFile = Path.Combine(DefaultOutputDir, nameof(OverWrite_CreatesANewFile)) + Compressor.OutputExtension;
            CreateZipFile(expectedZipFile);
            Ensure.True(File.Exists(expectedZipFile));

            // Act
            var options = new ZipOptions()
            {
                OverWrite = true,
                Input = InputDir,
                Output = expectedZipFile
            };

            var exitCode = _compressor.Run(options);

            // Assert - final file doesn't include original file's content
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(expectedZipFile));

            using var fs = new FileStream(expectedZipFile, FileMode.Open);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read);

            Assert.NotEmpty(zip.Entries);
        }

        [Fact]
        public void NoOverwrite_ThrowsAnException()
        {
            // Arrange - create a pre-existing 'output' file
            var expectedZipFile = Path.Combine(DefaultOutputDir, nameof(NoOverwrite_ThrowsAnException)) + Compressor.OutputExtension;
            CreateZipFile(expectedZipFile);
            Ensure.True(File.Exists(expectedZipFile));

            // Act
            var options = new ZipOptions()
            {
                OverWrite = false,
                Input = InputDir,
                Output = expectedZipFile
            };

            var exitCode = _compressor.Run(options);

            // Assert - final file is initial file
            Assert.Equal(1, exitCode);
            Assert.True(File.Exists(expectedZipFile));

            using var fs = new FileStream(expectedZipFile, FileMode.Open);
            using ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Read);

            Assert.Empty(zip.Entries);
        }

        private static void CreateZipFile(string filePath)
        {
            using var fs = new FileStream(filePath, FileMode.OpenOrCreate);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Create);
        }
    }
}
