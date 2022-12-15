using DotnetCombine.Options;
using DotnetCombine.Services;
using Xunit;

namespace DotnetCombine.Test.CombinerTests;

public class PreprocessorAtEoFTests : BaseCombinerTests
{
    [Fact]
    public async Task Endif()
    {
        // Arrange
        var initialCsFile = Path.Combine(DefaultOutputDir, $"{nameof(Endif)}-input" + Combiner.OutputExtension);
        var outputCsFile = Path.Combine(DefaultOutputDir, $"{nameof(Endif)}-output" + Combiner.OutputExtension);

        const string originalContent = @"
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System.Reflection;

//Lynx.Benchmark.BitBoard_Struct_ReadonlyStruct_Class_Record.SizeTest();

#if DEBUG
BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args, new DebugInProcessConfig());
#else
            BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run(args);
#endif
";

        CreateFile(initialCsFile, originalContent);

        // Act
        var options = new CombineOptions()
        {
            Output = outputCsFile,
            OverWrite = true,
            Input = initialCsFile
        };

        var exitCode = await new Combiner(options).Run();

        // Assert
        Assert.Equal(0, exitCode);

        var oldContent = (await File.ReadAllLinesAsync(initialCsFile));
        var newContent = (await File.ReadAllLinesAsync(outputCsFile)).Where(l => l.Length > 0);

        Assert.Equal(oldContent.Last(), newContent.Last());
        Assert.Contains("#endif", newContent.Last());
    }
}
