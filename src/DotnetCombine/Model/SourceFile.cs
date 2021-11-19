using DotnetCombine.SyntaxRewriters;
using DotnetCombine.Options;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotnetCombine.Model;

internal class SourceFile
{
    private CompilationUnitSyntax _root = null!;

    public string Filepath { get; }

    public IEnumerable<string> Usings => _root.Usings.Select(u => u.ToFullString()).Distinct();

    public IEnumerable<string> Code => _root.Members.Select(m => m.ToFullString());

    public string? Namespace => _root.DescendantNodes().OfType<NamespaceDeclarationSyntax>()?.FirstOrDefault()?.Name.ToString();

    public SourceFile(string filePath)
    {
        Filepath = filePath;
    }

    public async Task Parse(CombineOptions options)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(await File.ReadAllTextAsync(Filepath));
        CheckDiagnostics(syntaxTree.GetDiagnostics());

        var originalRoot = syntaxTree.GetCompilationUnitRoot();

        var pathToTrim = Directory.Exists(options.Input)
            ? options.Input.ReplaceEndingDirectorySeparatorWithProperEndingDirectorySeparator()
            : string.Empty;

        var syntaxNode = new AnnotateNamespacesRewriter(Filepath[pathToTrim.Length..]).Visit(originalRoot);

        _root = syntaxNode.SyntaxTree.GetCompilationUnitRoot();
    }

    private void CheckDiagnostics(IEnumerable<Diagnostic> diagnostics)
    {
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);

        if (errors.Any())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Errors detected in {Filepath}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }

        var warnings = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);
        if (warnings.Any())
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warnings detected in {Filepath}:{Environment.NewLine}{string.Join(Environment.NewLine, warnings)}");
        }
    }
}
