using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
	public partial interface IListItemClient
	{
		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task<ListItemDetail> CreateAsync(ListItemCreateRequest listItem, bool resolve = false, int timeout = 60000);

		ListItemDetail Create(ListItemCreateRequest listItem, bool resolve = false, int timeout = 60000);

		Task DeleteAsync(string objectId, CancellationToken cancellationToken = default(CancellationToken));

		void Delete(string objectId);

		ListItemDetail Update(string objectId, ListItemUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000);

		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task<ListItemDetail> UpdateAsync(string objectId, ListItemUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000, CancellationToken cancellationToken = default(CancellationToken));

		Task<IEnumerable<ListItem>> CreateFromPOCOAsync(object obj, string schemaId);

		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task<IEnumerable<ListItem>> CreateManyAsync(IEnumerable<ListItemCreateRequest> listItems, CancellationToken? cancellationToken = null);

		Task UpdateListItemAsync(ListItemUpdateRequest updateRequest);

		Task UpdateListItemAsync(ListItemDetail listItem, object obj, string schemaId);

		Task UpdateListItemAsync(ListItem listItem, object obj, string schemaId);

		Task<T> GetObjectAsync<T>(string objectId, string schemaId);
	}
}