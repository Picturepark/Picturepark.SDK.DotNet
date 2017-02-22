using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Clients;
using Newtonsoft.Json.Linq;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Authentication;

namespace Picturepark.SDK.V1
{
    public class JsonSchemaClient : JsonSchemasClientBase
    {
        public JsonSchemaClient(string baseUrl, IAuthClient configuration) : base(configuration)
        {
            BaseUrl = baseUrl;
        }

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public async Task<JObject> GetAsync(string schemaId, CancellationToken? cancellationToken = null)
        {
            return await GetCoreAsync(schemaId, cancellationToken ?? CancellationToken.None) as JObject;
        }

        /// <exception cref="ApiException">A server side error occurred.</exception>
        public JObject Get(string schemaId)
        {
            return Task.Run(async () => await GetAsync(schemaId)).GetAwaiter().GetResult();
        }
    }
}
