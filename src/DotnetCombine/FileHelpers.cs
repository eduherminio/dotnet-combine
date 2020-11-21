using System.IO;

namespace DotnetCombine
{
    public static class FileHelpers
    {
        public static string ReplaceEndingDirectorySeparatorWithProperEndingDirectorySeparator(this string dirPath)
        {
            return Path.TrimEndingDirectorySeparator(dirPath) + Path.DirectorySeparatorChar;
        }
    }
}
