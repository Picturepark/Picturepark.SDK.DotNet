using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ContentPermissionSetsFixture : ClientFixture
    {
        private readonly ConcurrentQueue<string> _createdPermissionSetIds = new ConcurrentQueue<string>();

        public async Task<IReadOnlyList<ContentPermissionSetDetail>> CreatePermissionSets(int count)
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
                _createdPermissionSetIds.Enqueue(set.Id);
            }

            return createdSets.ToArray();
        }

        public async Task<ContentPermissionSetDetail> CreatePermissionSet()
        {
            return (await CreatePermissionSets(1)).Single();
        }

        public override void Dispose()
        {
            if (!_createdPermissionSetIds.IsEmpty)
            {
                foreach (var id in _createdPermissionSetIds)
                {
                    try
                    {
                        Client.ContentPermissionSet.DeleteAsync(id).GetAwaiter().GetResult();
                    }
                    catch (PermissionSetNotFoundException)
                    {
                        // ignored
                    }
                }
            }

            base.Dispose();
        }
    }
}