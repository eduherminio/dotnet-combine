using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DotnetCombine.Test.CombinerTests
{
    public class RoslynTests
    {
        [Fact]
        public void Test()
        {
            const string programText =
@"using System;
using System.Collections.Generic;
using System.Text;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}";

            var tree = CSharpSyntaxTree.ParseText(programText);

            var root = tree.GetCompilationUnitRoot();

            var compilation = CSharpCompilation.Create("HelloWorld")
                .AddReferences(MetadataReference.CreateFromFile(
                    typeof(string).Assembly.Location))
                .AddSyntaxTrees(tree);

            var usings = root.Usings;
            var systemName = usings[0].Name;

            // Use the semantic model for symbol information:
            var model = compilation.GetSemanticModel(tree);
            SymbolInfo nameInfo = model.GetSymbolInfo(systemName);
            var systemSymbol = (INamespaceSymbol?)nameInfo.Symbol;
            foreach (INamespaceSymbol ns in systemSymbol?.GetNamespaceMembers() ?? Enumerable.Empty<INamespaceSymbol>())
            {
                Console.WriteLine(ns);
            }
        }
    }
}
