﻿using DotnetCombine.Options;
using DotnetCombine.Services;
using SheepTools;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DotnetCombine.Test.CombinerTests
{
    public class OverwriteTests : BaseCombinerTests
    {
        [Fact]
        public async Task OverWrite_CreatesANewFile()
        {
            // Arrange - create a pre-existing 'output' file
            var initialCsFile = Path.Combine(DefaultOutputDir, nameof(OverWrite_CreatesANewFile)) + Combiner.OutputExtension;

            CreateFile(initialCsFile);
            Ensure.True(File.Exists(initialCsFile));

            // Act
            var options = new CombineOptions()
            {
                OverWrite = true,
                Input = InputDir,
                Output = initialCsFile
            };

            var exitCode = await _combiner.Run(options);

            // Assert - final file isn't initial file
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(initialCsFile));
            Assert.NotEmpty(File.ReadAllLines(initialCsFile));
        }

        [Fact]
        public async Task NoOverwrite_ThrowsAnException()
        {
            // Arrange - create a pre-existing 'output' file
            var initialCsFile = Path.Combine(DefaultOutputDir, nameof(NoOverwrite_ThrowsAnException)) + Combiner.OutputExtension;

            CreateFile(initialCsFile);
            Ensure.True(File.Exists(initialCsFile));

            // Act
            var options = new CombineOptions()
            {
                OverWrite = false,
                Input = InputDir,
                Output = initialCsFile
            };

            var exitCode = await _combiner.Run(options);

            // Assert - final file is initial file
            Assert.Equal(1, exitCode);
            Assert.True(File.Exists(initialCsFile));

            Assert.Empty(File.ReadAllLines(initialCsFile));
        }

        private static void CreateFile(string filePath)
        {
            File.CreateText(filePath).Dispose();
        }
    }
}