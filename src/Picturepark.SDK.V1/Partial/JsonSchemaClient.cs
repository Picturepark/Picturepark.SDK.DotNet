using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    public partial class JsonSchemaClient
    {
        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<JObject> GetAsync(string schemaId, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await GetCoreAsync(schemaId, cancellationToken) as JObject;
        }

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public JObject Get(string schemaId)
        {
            return Task.Run(async () => await GetAsync(schemaId)).GetAwaiter().GetResult();
        }
    }
}
