using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotnetCombine.SyntaxRewriters;

internal class AnnotateNamespacesRewriter : BaseCustomRewriter
{
    public AnnotateNamespacesRewriter(string message) : base(message) { }

    public override SyntaxNode? VisitFileScopedNamespaceDeclaration(FileScopedNamespaceDeclarationSyntax node)
    {
        var namespaceDeclaration = ConvertNamespaceTransform.ConvertFileScopedNamespace(node);
        var nodeWithComment = AddComment(namespaceDeclaration);

        return base.VisitNamespaceDeclaration(nodeWithComment);
    }

    public override SyntaxNode? VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        var nodeWithComment = AddComment(node);

        return base.VisitNamespaceDeclaration(nodeWithComment)!;
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
