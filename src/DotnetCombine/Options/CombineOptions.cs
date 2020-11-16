using CommandLine;
using System;
using System.Collections.Generic;

namespace DotnetCombine.Options
{
    [Verb("single-file", isDefault: false, HelpText = "Combines multiple source code files (.cs) into a single one")]
    public class CombineOptions
    {
    }
}
