using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class SchemaPermissionSetsFixture : ClientFixture
    {
        private readonly ConcurrentQueue<string> _createdPermissionSetIds = new ConcurrentQueue<string>();

        public async Task<IReadOnlyList<SchemaPermissionSetDetail>> CreatePermissionSets(int count)
        {
            var createRequests = Enumerable.Range(0, count).Select(_ =>
                new SchemaPermissionSetCreateRequest
                {
                    Names = new TranslatedStringDictionary
                    {
                        { "en", $"Schema_ps_test_{Guid.NewGuid()}" }
                    }
                }).ToArray();

            var created = await Client.SchemaPermissionSet.CreateManyAsync(new SchemaPermissionSetCreateManyRequest { Items = createRequests }).ConfigureAwait(false);

            var createdSets = await Client.SchemaPermissionSet.GetManyAsync(created.Rows.Select(r => r.Id)).ConfigureAwait(false);

            foreach (var set in createdSets)
            {
                _createdPermissionSetIds.Enqueue(set.Id);
            }

            return createdSets.ToArray();
        }

        public async Task<SchemaPermissionSetDetail> CreatePermissionSet()
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
                        Client.SchemaPermissionSet.DeleteAsync(id).GetAwaiter().GetResult();
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