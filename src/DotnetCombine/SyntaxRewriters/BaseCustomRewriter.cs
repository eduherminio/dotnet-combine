using Microsoft.CodeAnalysis.CSharp;

namespace DotnetCombine.SyntaxRewriters
{
    internal class BaseCustomRewriter : CSharpSyntaxRewriter
    {
        protected readonly string _message;

        public BaseCustomRewriter(string message)
        {
            _message = message;
        }
    }
}
