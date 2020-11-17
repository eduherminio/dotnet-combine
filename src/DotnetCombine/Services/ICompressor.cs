using DotnetCombine.Options;

namespace DotnetCombine.Services
{
    public interface ICompressor
    {
        int Run(ZipOptions options);
    }
}
