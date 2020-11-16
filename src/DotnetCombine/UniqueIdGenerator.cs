using System;

namespace DotnetCombine
{
    internal static class UniqueIdGenerator
    {
        public const string DateFormat = "yyyy'-'MM'-'dd'__'HH'_'mm'_'ss";
        public static string UniqueId() => $"{DateTime.Now.ToLocalTime().ToString(DateFormat)}";
    }
}
