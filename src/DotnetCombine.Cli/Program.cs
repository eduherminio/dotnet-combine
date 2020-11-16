using CommandLine;
using DotnetCombine.Options;
using DotnetCombine.Services;

namespace DotnetCombine.Cli
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<ZipOptions>(args)
                .MapResult(
                  (ZipOptions options) => new Compressor().Run(options),
                  _ => 1);
        }
    }
}
