using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.FilescopedNanesoace;

public class ClassWithinFilescopedNanesoace
{
    /// <summary>
    /// Privaet constructor
    /// </summary>
    private ClassWithinFilescopedNanesoace()
    {

    }

    public static ClassWithinFilescopedNanesoace GetInstance() => new ClassWithinFilescopedNanesoace();
}


public record struct RecordStructWithinFilescopedNanesoace(int A, float B);

public record class RecordClassWithinFilescopedNanesoace(int C, string D)
{
    public List<string> PublicList { get; }

    public RecordClassWithinFilescopedNanesoace() : this(1, "2")
    {
        PublicList = new() { "a", "b", "c" };
    }
}