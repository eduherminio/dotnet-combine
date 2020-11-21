using DotnetCombine.Services;
using System;
using System.IO;
using System.Linq;

namespace DotnetCombine.Test.CombinerTests
{
    public class CombinerTestsFixture : IDisposable
    {
        public const string InputDir = "TestFiles/Combiner";
        public const string DefaultOutputDir = "CombinerTests";
        public const string DefaultSuffix = "-generated";

        public CombinerTestsFixture()
        {
            CleanInputDir();
            CleanDefaultOutputDir();
            Directory.CreateDirectory(DefaultOutputDir);
        }

        protected static void CleanInputDir()
        {
            foreach (var file in Directory.GetFiles(InputDir)
                .Where(f => Path.GetExtension(f) == Combiner.OutputExtension && Path.GetFileName(f).Contains(DefaultSuffix, StringComparison.OrdinalIgnoreCase)))
            {
                File.Delete(file);
            }
        }

        protected static void CleanDefaultOutputDir()
        {
            if (Directory.Exists(DefaultOutputDir))
            {
                //Directory.Delete(DefaultOutputDir, recursive: true);
            }
        }

        #region IDisposable implementation

        protected virtual void Dispose(bool disposing)
        {
            CleanInputDir();

            CleanDefaultOutputDir();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
