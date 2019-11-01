using System.Collections.Concurrent;

namespace Picturepark.ServiceProvider.Example.BusinessProcess.Util
{
    internal class ContentIdQueue : BlockingCollection<string>
    {
    }
}