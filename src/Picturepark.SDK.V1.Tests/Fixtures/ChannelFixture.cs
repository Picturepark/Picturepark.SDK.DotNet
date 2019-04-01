using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ChannelFixture : ClientFixture
    {
        private readonly ConcurrentQueue<string> _createdChannelIds = new ConcurrentQueue<string>();

        public async Task<IReadOnlyList<Channel>> CreateChannels(int count)
        {
            var createRequests = Enumerable.Range(0, count).Select(_ =>
                new ChannelCreateRequest
                {
                    Names = new TranslatedStringDictionary
                    {
                        { "en", $"Channel_test_{Guid.NewGuid()}" }
                    },
                    SearchIndexId = "RootContentSearchIndex",
                    ViewForAll = false
                }).ToArray();

            var tasks = createRequests.Select(r => Client.Channel.CreateAsync(r));

            var created = await Task.WhenAll(tasks).ConfigureAwait(false);

            foreach (var set in created)
            {
                _createdChannelIds.Enqueue(set.Id);
            }

            return created.ToArray();
        }

        public async Task<Channel> CreateChannel()
        {
            return (await CreateChannels(1)).Single();
        }

        public override void Dispose()
        {
            if (!_createdChannelIds.IsEmpty)
            {
                foreach (var id in _createdChannelIds)
                {
                    try
                    {
                        Client.Channel.DeleteAsync(id).GetAwaiter().GetResult();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            base.Dispose();
        }
    }
}