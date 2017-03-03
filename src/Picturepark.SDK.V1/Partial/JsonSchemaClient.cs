using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
    public partial class JsonSchemaClient
    {
        public JsonSchemaClient(string baseUrl, IAuthClient authClient) : this(authClient)
        {
            BaseUrl = baseUrl;
        }

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
