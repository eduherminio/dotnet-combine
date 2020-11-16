using DotnetCombine.Options;
using System;
using Xunit;

namespace DotnetCombine.Test.CompressorTests
{
    public class ValidationTests : BaseCompressorTests
    {
        [Fact]
        public void NoSource_ShouldFail()
        {
            // Arrange
            var options = new ZipOptions();

            // Act and assert
            Assert.Throws<ArgumentException>(() => _compressor.Run(options));
        }
    }
}
