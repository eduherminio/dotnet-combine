// Adapted from: https://github.com/dotnet/roslyn/blob/main/src/Analyzers/CSharp/CodeFixes/ConvertNamespace/ConvertNamespaceTransform.cs
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace DotnetCombine;

internal static class ConvertNamespaceTransform
{
    internal static NamespaceDeclarationSyntax ConvertFileScopedNamespace(FileScopedNamespaceDeclarationSyntax fileScopedNamespace)
    {
        var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(
            fileScopedNamespace.AttributeLists,
            fileScopedNamespace.Modifiers,
            fileScopedNamespace.NamespaceKeyword,
            fileScopedNamespace.Name,
            SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithTrailingTrivia(fileScopedNamespace.SemicolonToken.TrailingTrivia),
            fileScopedNamespace.Externs,
            fileScopedNamespace.Usings,
            fileScopedNamespace.Members,
            SyntaxFactory.Token(SyntaxKind.CloseBraceToken),
            semicolonToken: default).WithAdditionalAnnotations(Formatter.Annotation);

        // Ensure there is no errant blank line between the open curly and the first body element.
        var firstBodyToken = namespaceDeclaration.OpenBraceToken.GetNextToken();
        if (firstBodyToken != namespaceDeclaration.CloseBraceToken &&
            !firstBodyToken.IsKind(SyntaxKind.EndOfFileToken) &&
            HasLeadingBlankLine(firstBodyToken, out var firstBodyTokenWithoutBlankLine))
        {
            namespaceDeclaration = namespaceDeclaration.ReplaceToken(firstBodyToken, firstBodyTokenWithoutBlankLine);
        }

        return namespaceDeclaration;
    }

    private static bool HasLeadingBlankLine(SyntaxToken token, out SyntaxToken withoutBlankLine)
    {
        var leadingTrivia = token.LeadingTrivia;

        if (leadingTrivia.FirstOrDefault().IsKind(SyntaxKind.EndOfLineTrivia))
        {
            withoutBlankLine = token.WithLeadingTrivia(leadingTrivia.RemoveAt(0));
            return true;
        }

        if (leadingTrivia.ElementAtOrDefault(0).IsKind(SyntaxKind.WhitespaceTrivia) && leadingTrivia.ElementAtOrDefault(1).IsKind(SyntaxKind.EndOfLineTrivia))
        {
            withoutBlankLine = token.WithLeadingTrivia(leadingTrivia.Skip(2));
            return true;
        }

        withoutBlankLine = default;
        return false;
    }
}