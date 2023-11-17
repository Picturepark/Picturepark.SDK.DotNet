using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Helpers
{
    public class ContentPermissionSetsEntityCreator : EntityCreatorBase<ContentPermissionSetDetail>
    {
        public ContentPermissionSetsEntityCreator(IPictureparkService client) : base(client)
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

            var created = await Client.ContentPermissionSet.CreateManyAsync(new ContentPermissionSetCreateManyRequest { Items = createRequests });

            var createdSets = await Client.ContentPermissionSet.GetManyAsync(created.Rows.Select(r => r.Id));

            AddCreated(createdSets.Select(s => s.Id));

            return createdSets.ToArray();
        }

        protected override Task Delete(string id) => Client.ContentPermissionSet.DeleteAsync(id);
    }
}