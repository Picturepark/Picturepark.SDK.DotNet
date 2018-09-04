using Picturepark.Microsite.Example.Services;
using Picturepark.SDK.V1.Contract;
using System;
using System.Threading.Tasks;

namespace Picturepark.Microsite.Example.Helpers
{
	public class ServiceHelper : IServiceHelper
	{
		private readonly IPictureparkAccessTokenService _client;

		public ServiceHelper(IPictureparkAccessTokenService client)
		{
			_client = client;
		}

		public async Task EnsureSchemaExists<T>(Action<SchemaDetail> beforeCreateOrUpdateAction, bool updateSchema)
		{
			// Ensure that schema exists in PCP
			var schemas = await _client.Schema.GenerateSchemasAsync(typeof(T));

			foreach (var schema in schemas)
			{
				beforeCreateOrUpdateAction?.Invoke(schema);

				if (!updateSchema)
				{
					if (await _client.Schema.ExistsAsync(schema.Id) == false)
						await _client.Schema.CreateAsync(schema, true);
				}
				else
				{
					await _client.Schema.CreateOrUpdateAsync(schema, true);
				}
			}
		}
	}
}
