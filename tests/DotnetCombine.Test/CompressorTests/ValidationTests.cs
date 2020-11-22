using DotnetCombine.Options;
using DotnetCombine.Services;
using System;
using Xunit;

namespace DotnetCombine.Test.CompressorTests
{
    public class ValidationTests : BaseCompressorTests
    {
        [Fact]
        public void NoInput_ShouldFail()
        {
            // Arrange
            var options = new ZipOptions();

            // Act and assert
            Assert.Equal(1, new Compressor(options).Run());
        }

        [Fact]
        public void NonExistingDirInput_ShouldFail()
        {
            // Arrange
            var options = new ZipOptions()
            {
                Input = "./___non_existing_dir___"
            };

            // Act and assert
            Assert.Equal(1, new Compressor(options).Run());
        }
    }
}
