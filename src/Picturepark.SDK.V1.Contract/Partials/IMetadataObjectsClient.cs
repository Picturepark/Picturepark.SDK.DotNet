using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IMetadataObjectsClient
    {
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<MetadataObjectDetailViewItem> CreateAsync(MetadataObjectCreateRequest metadataObject, bool resolve = false, int timeout = 60000);

        MetadataObjectDetailViewItem Create(MetadataObjectCreateRequest metadataObject, bool resolve = false, int timeout = 60000);

        Task<MetadataObjectViewItem> CreateAbcAsync(MetadataObjectCreateRequest createRequest);

        Task DeleteAsync(string objectId, CancellationToken cancellationToken = default(CancellationToken));

        void Delete(string objectId);

        MetadataObjectDetailViewItem Update(string objectId, MetadataObjectUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<MetadataObjectDetailViewItem> UpdateAsync(string objectId, MetadataObjectUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<MetadataObjectViewItem>> CreateFromPOCO(object obj, string schemaId);

        IEnumerable<MetadataObjectViewItem> CreateMany(IEnumerable<MetadataObjectCreateRequest> objects);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<IEnumerable<MetadataObjectViewItem>> CreateManyAsync(IEnumerable<MetadataObjectCreateRequest> metadataObjects, CancellationToken? cancellationToken = null);

        Task UpdateMetadataObjectAsync(MetadataObjectUpdateRequest updateRequest);

        Task UpdateMetadataObjectAsync(MetadataObjectDetailViewItem metadataObject, object obj, string schemaId);

        Task UpdateMetadataObjectAsync(MetadataObjectViewItem metadataObject, object obj, string schemaId);

        Task<T> GetObjectAsync<T>(string objectId);

        Task ImportFromJsonAsync(string jsonFilePath, bool includeObjects);
    }
}