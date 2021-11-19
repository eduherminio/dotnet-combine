using CommandLine;
using CommandLine.Text;
using DotnetCombine.Options;
using DotnetCombine.Services;
using System.Reflection;

namespace DotnetCombine;

public static class Program
{
    public static int Main(string[] args)
    {
        var parser = new Parser(config =>
        {
            config.HelpWriter = null;
            config.EnableDashDash = true;
        });

        var result = parser.ParseArguments<CombineOptions, ZipOptions>(args);

        return result.MapResult(
            (CombineOptions options) => new Combiner(options).Run().Result,
            (ZipOptions options) => new Compressor(options).Run(),
            _ => DisplayHelp(result));
    }

    private static int DisplayHelp(ParserResult<object> result)
    {
        var helpText = HelpText.AutoBuild(result, helpText =>
        {
            var assembly = Assembly.GetEntryAssembly();
            helpText.Heading = $"{assembly?.GetName().Name} {assembly?.GetName().Version}";
            helpText.Copyright = "By Eduardo Cáceres - https://github.com/eduherminio/dotnet-combine";
            helpText.MaximumDisplayWidth = 120;
            helpText.AddNewLineBetweenHelpSections = true;
            helpText.AdditionalNewLineAfterOption = true;

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

            return helpText;
        });

        Console.WriteLine(helpText);

        return 1;
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
