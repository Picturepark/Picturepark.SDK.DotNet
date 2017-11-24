using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
	public partial interface IListItemClient
	{
		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task<ListItemDetail> CreateAsync(ListItemCreateRequest listItem, bool resolve = false, int timeout = 60000, CancellationToken cancellationToken = default(CancellationToken));

		ListItemDetail Create(ListItemCreateRequest listItem, bool resolve = false, int timeout = 60000);

		Task DeleteAsync(string objectId, CancellationToken cancellationToken = default(CancellationToken));

		void Delete(string objectId);

		ListItemDetail Update(string objectId, ListItemUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000);

		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task<ListItemDetail> UpdateAsync(string objectId, ListItemUpdateRequest updateRequest, bool resolve = false, List<string> patterns = null, int timeout = 60000, CancellationToken cancellationToken = default(CancellationToken));

		Task UpdateAsync(ListItemUpdateRequest updateRequest, CancellationToken cancellationToken = default(CancellationToken));

		Task UpdateAsync(ListItemDetail listItem, object obj, string schemaId, CancellationToken cancellationToken = default(CancellationToken));

		Task UpdateAsync(ListItem listItem, object obj, string schemaId, CancellationToken cancellationToken = default(CancellationToken));

		Task<IEnumerable<ListItem>> CreateFromPOCOAsync(object obj, string schemaId, CancellationToken cancellationToken = default(CancellationToken));

		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task<IEnumerable<ListItem>> CreateManyAsync(IEnumerable<ListItemCreateRequest> listItems, CancellationToken cancellationToken = default(CancellationToken));

		Task<T> GetObjectAsync<T>(string objectId, string schemaId, CancellationToken cancellationToken = default(CancellationToken));
	}
}