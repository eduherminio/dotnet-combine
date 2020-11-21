using DotnetCombine.Services;
using Xunit;

namespace DotnetCombine.Test.CombinerTests
{
    /// <summary>
    /// This class has no code, and is never created. Its purpose is simply
    /// to be the place to apply [CollectionDefinition] and all the
    /// ICollectionFixture interfaces.
    /// </summary>
    [CollectionDefinition(nameof(CombinerTestsFixture))]
    public class CombinerTestsCollection : ICollectionFixture<CombinerTestsFixture> { }

    [Collection(nameof(CombinerTestsFixture))]
    public class BaseCombinerTests
    {
        protected const string InputDir = CombinerTestsFixture.InputDir;
        protected const string DefaultOutputDir = CombinerTestsFixture.DefaultOutputDir;

        protected readonly Combiner _combiner;

        public BaseCombinerTests()
        {
            _combiner = new Combiner();
        }
    }
}