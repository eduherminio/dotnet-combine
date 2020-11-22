using DotnetCombine.Options;
using DotnetCombine.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DotnetCombine.Test.CombinerTests.OptionsTests
{
    public class ValidationTests : BaseCombinerTests
    {
        [Fact]
        public async Task NoInput_ShouldFail()
        {
            // Arrange
            var options = new CombineOptions();

            // Act and assert
            Assert.Equal(1, await new Combiner(options).Run());
        }

        [Fact]
        public void NonExistingDirInput_ShouldFail()
        {
            // Arrange
            var options = new ZipOptions()
            {
                Input = "./___non_existing_dir___/"
            };

            // Act and assert
            Assert.Equal(1, new Compressor(options).Run());
        }

        [Fact]
        public void NonExistingFileInput_ShouldFail()
        {
            // Arrange
            var options = new ZipOptions()
            {
                Input = "./___non_existing_file___"
            };

            // Act and assert
            Assert.Equal(1, new Compressor(options).Run());
        }
    }
}
