using System.IO;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1
{
    public static class StreamExtensions
    {
        public static async Task WriteToFileAsync(this Stream stream, string filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                await stream.CopyToAsync(fileStream);
            }
        }
    }
}