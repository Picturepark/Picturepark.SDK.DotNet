using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Helpers
{
    public abstract class PermissionSetsHelper<TDetail> : IDisposable
    {
        protected PermissionSetsHelper(IPictureparkService client)
        {
            Client = client;
        }

        protected IPictureparkService Client { get; }

        protected ConcurrentQueue<string> CreatedPermissionSetIds { get; } = new ConcurrentQueue<string>();

        public abstract Task<IReadOnlyList<TDetail>> Create(int count);

        public async Task<TDetail> Create()
        {
            return (await Create(1)).Single();
        }

        public void Dispose()
        {
            if (!CreatedPermissionSetIds.IsEmpty)
            {
                foreach (var id in CreatedPermissionSetIds)
                {
                    try
                    {
                        Delete(id).GetAwaiter().GetResult();
                    }
                    catch (PermissionSetNotFoundException)
                    {
                        // ignored
                    }
                }
            }
        }

        protected abstract Task Delete(string id);
    }

    public class ContentPermissionSetsHelper : PermissionSetsHelper<ContentPermissionSetDetail>
    {
        public ContentPermissionSetsHelper(IPictureparkService client) : base(client)
        {
        }

        public override async Task<IReadOnlyList<ContentPermissionSetDetail>> Create(int count)
        {
            var createRequests = Enumerable.Range(0, count).Select(_ =>
                new ContentPermissionSetCreateRequest
                {
                    Names = new TranslatedStringDictionary
                    {
                        { "en", $"Content_ps_test_{Guid.NewGuid()}" }
                    }
                }).ToArray();

            var created = await Client.ContentPermissionSet.CreateManyAsync(new ContentPermissionSetCreateManyRequest { Items = createRequests }).ConfigureAwait(false);

            var createdSets = await Client.ContentPermissionSet.GetManyAsync(created.Rows.Select(r => r.Id)).ConfigureAwait(false);

            foreach (var set in createdSets)
            {
                CreatedPermissionSetIds.Enqueue(set.Id);
            }

            return createdSets.ToArray();
        }

        protected override Task Delete(string id) => Client.ContentPermissionSet.DeleteAsync(id);
    }
}