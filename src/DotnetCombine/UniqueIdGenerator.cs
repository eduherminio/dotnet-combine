using DotnetCombine.Services;
using System.Text.RegularExpressions;

namespace DotnetCombine;

internal static class UniqueIdGenerator
{
    public const string DateFormat = "yyyy'-'MM'-'dd'__'HH'_'mm'_'ss";

    public static readonly Regex GeneratedFileNameRegex = new(@".*\d{4}-\d{2}-\d{2}__\d{2}_\d{2}_\d{2}.*\" + Combiner.OutputExtension,
        RegexOptions.ExplicitCapture | RegexOptions.Compiled);

    public static string UniqueId() => DateTime.Now.ToLocalTime().ToString(DateFormat);
}
