using System.Collections.Concurrent;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ShareFixture : ClientFixture
    {
        public ConcurrentQueue<string> CreatedShareIds { get; set; } = new ConcurrentQueue<string>();

        public override void Dispose()
        {
            if (!CreatedShareIds.IsEmpty)
                Client.Share.DeleteManyAsync(new ShareDeleteManyRequest { Ids = CreatedShareIds.ToArray() }).GetAwaiter().GetResult();

            base.Dispose();
        }
    }
}
