using Newtonsoft.Json.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
	public partial interface IJsonSchemaClient
	{
		Task<JObject> GetAsync(string schemaId, CancellationToken cancellationToken = default(CancellationToken));

		JObject Get(string schemaId);
	}
}
