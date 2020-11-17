using DotnetCombine.Services;
using Xunit;

namespace DotnetCombine.Test.CompressorTests
{
    [CollectionDefinition(nameof(CompressorTestsFixture))]
    public class CompressorTestsCollection : ICollectionFixture<CompressorTestsFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    [Collection(nameof(CompressorTestsFixture))]
    public class BaseCompressorTests
    {
        protected const int TotalCsFiles = CompressorTestsFixture.TotalCsFiles;
        protected const int TotalTxtFiles = CompressorTestsFixture.TotalTxtFiles;
        protected const string InputDir = CompressorTestsFixture.InputDir;
        protected const string DefaultOutputDir = CompressorTestsFixture.DefaultOutputDir;

        protected readonly ICompressor _compressor;

        public BaseCompressorTests()
        {
            _compressor = new Compressor();
        }
    }
}