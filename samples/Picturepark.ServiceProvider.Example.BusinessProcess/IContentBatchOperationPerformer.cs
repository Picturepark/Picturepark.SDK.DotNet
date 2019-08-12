using System.Collections.Generic;

namespace Picturepark.ServiceProvider.Example.BusinessProcess
{
    internal interface IContentBatchOperationPerformer
    {
        void EnqueueContentsById(IReadOnlyList<string> contentIds);
    }
}