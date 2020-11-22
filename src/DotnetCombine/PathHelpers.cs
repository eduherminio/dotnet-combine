using System.IO;

namespace DotnetCombine
{
    public static class PathHelpers
    {
        public static string ReplaceEndingDirectorySeparatorWithProperEndingDirectorySeparator(this string dirPath)
        {
            return Path.TrimEndingDirectorySeparator(dirPath) + Path.DirectorySeparatorChar;
        }
    }
}
