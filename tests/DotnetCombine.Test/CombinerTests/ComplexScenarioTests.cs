using DotnetCombine.Options;
using DotnetCombine.Services;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DotnetCombine.Test.CombinerTests
{
    public class ComplexScenarioTests : BaseCombinerTests, IDisposable
    {
        private string? _outputPath;

        [Fact]
        public async Task OutputFileContent()
        {
            // Arrange
            var input = "TestsInput/Combiner/ComplexScenario/";
            _outputPath = Path.Combine("ComplexScenarioTestsOutput", nameof(FileContentTest) + Combiner.OutputExtension);

            var options = new CombineOptions
            {
                Input = input,
                Output = _outputPath,
                OverWrite = true
            };

            // Act
            var exitCode = await _combiner.Run(options);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(_outputPath));

            CheckFileContent(_outputPath);
        }

        [Fact]
        public async Task OutputFileCompilation()
        {
            // Arrange
            var input = "TestsInput/Combiner/ComplexScenario/";
            _outputPath = Path.Combine("ComplexScenarioTestsOutput", nameof(OutputFileCompilation) + Combiner.OutputExtension);

            var options = new CombineOptions
            {
                Input = input,
                Output = _outputPath,
                OverWrite = true
            };

            // Act
            var exitCode = await _combiner.Run(options);

            // Assert
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(_outputPath));

            await CheckCompilationResults(_outputPath);
        }

        private static void CheckFileContent(string output)
        {
            var files = Directory.GetFiles(InputDir, $"*{Combiner.OutputExtension}", SearchOption.AllDirectories);

            var sourceLines = files.SelectMany(f => File.ReadAllLines(f));
            var outputFileLines = File.ReadAllLines(output);

            Assert.All(sourceLines, (sourceLine) => outputFileLines.Contains(sourceLine));
        }

        private async Task CheckCompilationResults(string output)
        {
            var compilationResults = await Compile(output);

            Assert.Empty(compilationResults.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

            // CS0105 : The using directive for 'xxxx' appeared previously in this namespace
            // CS0105 : Using directive is unnecesary
            Assert.Empty(compilationResults.Diagnostics.Where(d => d.Id != "CS0105" && d.Id != "CS8019"));

            Assert.True(compilationResults.Success);
        }

        private async Task<EmitResult> Compile(string filePath)
        {
            var options = new CSharpCompilationOptions(
                 OutputKind.ConsoleApplication,                     // Avoid CS8805
                 optimizationLevel: OptimizationLevel.Release,
                 allowUnsafe: true);

            var tree = CSharpSyntaxTree.ParseText(await File.ReadAllTextAsync(filePath));

            var references = new List<PortableExecutableReference>()
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),         // System.Private.CoreLib.dll (mscorelib)
                MetadataReference.CreateFromFile(typeof(HttpClient).Assembly.Location),     // System.Net.Http.dll
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),        // System.Console.dll
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location)      // System.Linq.dll
            };

            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            references.Add(MetadataReference.CreateFromFile(Path.Combine(assemblyPath!, "System.Runtime.dll")));

            var compilation = CSharpCompilation.Create(
                Path.GetFileName(filePath),
                syntaxTrees: new[] { tree },
                references: references,
                options: options);

            using var stream = new MemoryStream();
            return compilation.Emit(stream);
        }

        #region IDisposable implementation

        protected virtual void Dispose(bool disposing)
        {
            if (File.Exists(_outputPath))
            {
                File.Delete(_outputPath!);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
