using System.Collections.Concurrent;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ShareFixture : ClientFixture
    {
        public ConcurrentQueue<string> CreatedShareIds { get; set; } = new ConcurrentQueue<string>();

        public override void Dispose()
        {
            if (!CreatedShareIds.IsEmpty)
            {
                var deleteResult = Client.Shares.DeleteManyAsync(new ShareDeleteManyRequest { Ids = CreatedShareIds.ToArray() }).GetAwaiter().GetResult();
                deleteResult.Rows.Should().OnlyContain(r => r.Succeeded);
            }

            base.Dispose();
        }
    }
}
