using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Example.UsingsFolder
{
    internal class WithinUsingsFolder_UsingsWithoutBraces
    {
        void Foo()
        {
            using var fs = new FileStream("", FileMode.OpenOrCreate);
            using var sw = new StreamWriter(fs);
            sw.Close();
        }
    }
}
