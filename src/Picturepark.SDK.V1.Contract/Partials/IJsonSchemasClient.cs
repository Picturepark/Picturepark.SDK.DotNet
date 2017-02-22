using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IJsonSchemasClient
    {
        Task<JObject> GetAsync(string schemaId, CancellationToken cancellationToken = default(CancellationToken));

        JObject Get(string schemaId);
    }
}