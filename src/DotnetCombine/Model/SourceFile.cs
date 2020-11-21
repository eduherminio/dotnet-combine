using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SheepTools.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetCombine.Model
{
    internal class SourceFile
    {
        public ICollection<string> Usings { get; set; }

        public string Filepath { get; init; }

        public string? Namespace { get; set; }

        public ICollection<string> Includes { get; set; }


        public ICollection<string> Code { get; set; }

        public SourceFile(string filePath)
        {
            Filepath = filePath;
            Includes = new List<string>();
            Usings = new List<string>();
            Code = new List<string>();
        }

        public async Task Parse()
        {
            var lines = await File.ReadAllLinesAsync(Filepath);

            Includes = lines.TakeWhile(l =>
                    l.StartsWith("using ", StringComparison.OrdinalIgnoreCase)
                    || l.StartsWith("//")
                    || l.StartsWith("/*")
                    || l.StartsWith("*")
                    || l.StartsWith(" *")
                    || l.StartsWith("/*")
                    || l.IsWhiteSpace())
                .ToList();

            Code = lines.Skip(Includes.Count).ToList();

            Includes.RemoveAll(l => string.IsNullOrWhiteSpace(l));

            Namespace = Code.FirstOrDefault(l => l.StartsWith("namespace "));
        }
    }
}
