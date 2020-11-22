// Top comments
// More top comments

using System.IO;

namespace Example.UsingsFolder
{
    public class WithinUsingsFolder_TopCommentsLines
    {
        public void UsingsNested()
        {
            using (var fs = new FileStream("", FileMode.Open))
            {
                using (var sw = new StreamReader(fs))
                {
                    _ = sw.ReadToEnd();
                }
            }
        }
    }
}
