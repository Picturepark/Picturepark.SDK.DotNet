using System.IO;

namespace Picturepark.SDK.V1
{
    public static class StreamExtensions
    {
        // TODO: Make async
        public static void SaveFile(this Stream stream, string filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                stream.CopyTo(fileStream);
            }
        }
    }
}