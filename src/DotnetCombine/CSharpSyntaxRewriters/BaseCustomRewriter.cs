using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotnetCombine.CSharpSyntaxRewriters
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
