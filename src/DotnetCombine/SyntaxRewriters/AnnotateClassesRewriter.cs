using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace DotnetCombine.SyntaxRewriters
{
    internal class AnnotateClassesRewriter : BaseCustomRewriter
    {
        public AnnotateClassesRewriter(string message) : base(message) { }

        public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
            => AddComment(node);

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            => AddComment(node);

        public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
            => AddComment(node);

        public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
            => AddComment(node);

        public override SyntaxNode VisitRecordDeclaration(RecordDeclarationSyntax node)
            => AddComment(node);

        private TDeclarationSyntax AddComment<TDeclarationSyntax>(TDeclarationSyntax node)
            where TDeclarationSyntax : MemberDeclarationSyntax
        {
            var existingTrivia = node.GetLeadingTrivia();

            var leadingSpaces = new string(' ', existingTrivia.ToString().TakeWhile(ch => char.IsWhiteSpace(ch)).Count());
            var newTrivia = existingTrivia.Prepend(SyntaxFactory.Comment(
                $"{leadingSpaces}// {_message}" +
                $"{(existingTrivia.ToString().EndsWith("\n") ? "" : Environment.NewLine)}"));

            return node.WithLeadingTrivia(newTrivia);
        }
    }
}
