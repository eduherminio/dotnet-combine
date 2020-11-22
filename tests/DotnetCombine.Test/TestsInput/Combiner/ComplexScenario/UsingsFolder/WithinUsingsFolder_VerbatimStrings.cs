using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.UsingsFolder
{
    internal record Root_VerbatimStrings
    {
        public string Foo { get; set; } = @"The danger of @ strings if a line starts with
using";
    }
}
