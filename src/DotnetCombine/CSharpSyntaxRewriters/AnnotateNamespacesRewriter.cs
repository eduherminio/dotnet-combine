using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace DotnetCombine.CSharpSyntaxRewriters
{
    internal class AnnotateNamespacesRewriter : BaseCustomRewriter
    {
        public AnnotateNamespacesRewriter(string message) : base(message) { }

        public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            var newNode = AddComment(node);

            return base.VisitNamespaceDeclaration(newNode)!;
        }

        private NamespaceDeclarationSyntax AddComment(NamespaceDeclarationSyntax node)
        {
            var existingTrivia = node.GetLeadingTrivia();

            var newTrivia = existingTrivia.Prepend(SyntaxFactory.Comment($"// {_message}"));

            return node.WithLeadingTrivia(newTrivia);
        }
    }
}
