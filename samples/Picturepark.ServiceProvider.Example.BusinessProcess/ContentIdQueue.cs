using System.Collections.Concurrent;

namespace Picturepark.ServiceProvider.Example.BusinessProcess
{
    internal class ContentIdQueue : BlockingCollection<string>
    {
    }
}