using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotnetCombine.SyntaxRewriters;

internal class AnnotateNamespacesRewriter : BaseCustomRewriter
{
    public AnnotateNamespacesRewriter(string message) : base(message) { }

    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        var newNewNode = ConvertNamespaceTransform.ConvertFileScopedNamespace(node);
        var newNode = AddComment(newNewNode);
        return base.VisitNamespaceDeclaration(newNode);
    }

    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        var newNode = AddComment(node);

        return base.VisitNamespaceDeclaration(newNode)!;
    }

    private T AddComment<T>(T node)
        where T : BaseNamespaceDeclarationSyntax
    {
        SyntaxTrivia TriviaToAdd(SyntaxTriviaList? existingTrivia = null) => SyntaxFactory.Comment($"// {_message}" +
            $"{(existingTrivia?.ToString().EndsWith("\n") == true ? "" : Environment.NewLine)}");

        if (node.HasLeadingTrivia)
        {
            var existingTrivia = node.GetLeadingTrivia();

            return node.WithLeadingTrivia(existingTrivia.Prepend(TriviaToAdd(existingTrivia)));
        }

        return node.WithLeadingTrivia(TriviaToAdd());
    }
}
