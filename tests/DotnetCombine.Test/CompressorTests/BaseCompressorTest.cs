using Xunit;

namespace DotnetCombine.Test.CompressorTests
{
    /// <summary>
    /// This class has no code, and is never created. Its purpose is simply
    /// to be the place to apply [CollectionDefinition] and all the
    /// ICollectionFixture interfaces.
    /// </summary>
    [CollectionDefinition(nameof(CompressorTestsFixture))]
    public class CompressorTestsCollection : ICollectionFixture<CompressorTestsFixture> { }

    [Collection(nameof(CompressorTestsFixture))]
    public abstract class BaseCompressorTests
    {
        protected const int TotalCsFiles = CompressorTestsFixture.TotalCsFiles;
        protected const int TotalTxtFiles = CompressorTestsFixture.TotalTxtFiles;
        protected const string InputDir = CompressorTestsFixture.InputDir;
        protected const string DefaultOutputDir = CompressorTestsFixture.DefaultOutputDir;

        protected BaseCompressorTests() { }
    }
}