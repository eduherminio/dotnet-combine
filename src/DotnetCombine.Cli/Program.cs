using CommandLine;
using CommandLine.Text;
using DotnetCombine.Options;
using DotnetCombine.Services;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotnetCombine.Cli
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var parser = new Parser(config =>
            {
                config.AutoVersion = false;
                config.HelpWriter = null;
            });

            var result = parser.ParseArguments<CombineOptions, ZipOptions>(args);

            return result
                .WithNotParsed(_ => DisplayHelp(result))
                .MapResult(
                  (ZipOptions options) => new Compressor().Run(options),
                  _ => 1);
        }

        private static void DisplayHelp(ParserResult<object> result)
        {
            var helpText = HelpText.AutoBuild(result, helpText =>
            {
                var assembly = Assembly.GetEntryAssembly();
                helpText.Heading = $"{assembly?.GetName().Name} {assembly?.GetName().Version}";
                helpText.Copyright = "By Eduardo Cáceres - https://github.com/eduherminio/dotnet-combine";
                helpText.MaximumDisplayWidth = 120;
                helpText.AddNewLineBetweenHelpSections = true;
                helpText.AdditionalNewLineAfterOption = true;
                helpText.AutoVersion = false;

                helpText.OptionComparison = OrderWithValuesFirst;

                var typeUsage = new Dictionary<Type, string>
                {
                    [typeof(ZipOptions)] = "Usage: dotnet-combine zip <INPUT> [options]",
                    [typeof(CombineOptions)] = "Usage: dotnet-combine single-file <INPUT> [options]"
                };

                if (typeUsage.TryGetValue(result.TypeInfo.Current, out var usageMessage))
                {
                    helpText.AddPreOptionsLine(usageMessage);
                }

                return HelpText.DefaultParsingErrorsHandler(result, helpText);
            }, e => e);

            Console.WriteLine(helpText);
        }

        private static readonly Comparison<ComparableOption> OrderWithValuesFirst = (ComparableOption attr1, ComparableOption attr2) =>
        {
            if (attr1.IsOption && attr2.IsOption)
            {
                if (attr1.Required && !attr2.Required)
                {
                    return -1;
                }
                else if (!attr1.Required && attr2.Required)
                {
                    return 1;
                }

                return attr1.Index > attr2.Index
                    ? 1
                    : -1;
            }
            else if (attr1.IsOption && attr2.IsValue)
            {
                return 1;
            }
            else if (attr1.IsValue && attr2.IsOption)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        };
    }
}
