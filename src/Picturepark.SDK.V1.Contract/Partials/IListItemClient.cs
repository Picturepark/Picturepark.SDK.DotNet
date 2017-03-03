using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IListItemClient
    {
        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<ListItemDetailViewItem> CreateAsync(ListItemCreateRequest listItem, bool resolve = false, int timeout = 60000);

        ListItemDetailViewItem Create(ListItemCreateRequest listItem, bool resolve = false, int timeout = 60000);

        Task<ListItemViewItem> CreateAbcAsync(ListItemCreateRequest createRequest);

        Task DeleteAsync(string objectId, CancellationToken cancellationToken = default(CancellationToken));

        void Delete(string objectId);

        ListItemDetailViewItem Update(string objectId, ListItemUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<ListItemDetailViewItem> UpdateAsync(string objectId, ListItemUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<ListItemViewItem>> CreateFromPOCO(object obj, string schemaId);

        /// <exception cref="ApiException">A server side error occurred.</exception>
        Task<IEnumerable<ListItemViewItem>> CreateManyAsync(IEnumerable<ListItemCreateRequest> listItems, CancellationToken? cancellationToken = null);

        Task UpdateListItemAsync(ListItemUpdateRequest updateRequest);

        Task UpdateListItemAsync(ListItemDetailViewItem listItem, object obj, string schemaId);

        Task UpdateListItemAsync(ListItemViewItem listItem, object obj, string schemaId);

        Task<T> GetObjectAsync<T>(string objectId, string schemaId);

        Task ImportFromJsonAsync(string jsonFilePath, bool includeObjects);
    }
}