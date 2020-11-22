using DotnetCombine.Options;
using System;
using Xunit;

namespace DotnetCombine.Test.CombinerTests.OptionsTests
{
    public class ValidationTests : BaseCombinerTests
    {
        [Fact]
        public void NoSource_ShouldFail()
        {
            // Arrange
            var options = new CombineOptions();

            // Act and assert
            Assert.ThrowsAsync<ArgumentException>(() => _combiner.Run(options));
        }
    }
}
