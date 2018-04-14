using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    public partial class JsonSchemaClient
    {
        /// <summary>Gets an existing JSON Schema by schema ID.</summary>
        /// <param name="schemaId">The schema ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The JSON Schema as <see cref="JObject"/>.</returns>
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<JObject> GetAsync(string schemaId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetCoreAsync(schemaId, cancellationToken).ConfigureAwait(false) as JObject;
        }
    }
}
