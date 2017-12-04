using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
	public partial interface IJsonSchemaClient
	{
		/// <summary>Gets an existing JSON Schema by schema ID.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <returns>The JSON Schema as <see cref="JObject"/>.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		JObject Get(string schemaId);

		/// <summary>Gets an existing JSON Schema by schema ID.</summary>
		/// <param name="schemaId">The schema ID.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns>The JSON Schema as <see cref="JObject"/>.</returns>
		/// <exception cref="ApiException">A server side error occurred.</exception>
		Task<JObject> GetAsync(string schemaId, CancellationToken cancellationToken = default(CancellationToken));
	}
}
