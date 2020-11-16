using CommandLine;
using DotnetCombine.Options;
using DotnetCombine.Services;

namespace DotnetCombine.Cli
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<CombineOptions, ZipOptions>(args)
                .MapResult(
                  //(CombineOptions options) => new Combiner().Run(options),
                  (ZipOptions options) => new Compressor().Run(options),
                  _ => 1);
        }
    }
}
