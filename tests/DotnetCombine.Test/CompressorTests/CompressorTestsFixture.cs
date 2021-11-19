using DotnetCombine.Services;

namespace DotnetCombine.Test.CompressorTests;

public class CompressorTestsFixture : IDisposable
{
    public const int TotalCsFiles = 3;
    public const int TotalTxtFiles = 2;
    public const string InputDir = "TestsInput/Compressor";
    public const string DefaultOutputDir = "CompressorTestsOutput";

    public CompressorTestsFixture()
    {
        CleanInputDir();
        CleanDefaultOutputDir();
        Directory.CreateDirectory(DefaultOutputDir);
    }

    protected static void CleanInputDir()
    {
        foreach (var file in Directory.GetFiles(InputDir).Where(f => Path.GetExtension(f) == Compressor.OutputExtension))
        {
            File.Delete(file);
        }
    }

    protected static void CleanDefaultOutputDir()
    {
        if (Directory.Exists(DefaultOutputDir))
        {
            Directory.Delete(DefaultOutputDir, recursive: true);
        }
    }

    #region IDisposable implementation

    protected virtual void Dispose(bool disposing)
    {
        CleanInputDir();

        CleanDefaultOutputDir();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
