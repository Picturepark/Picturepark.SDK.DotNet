using Newtonsoft.Json.Linq;
using Picturepark.Microsite.Example.Contracts;
using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.Microsite.Example.Services;

namespace Picturepark.Microsite.Example.Helpers
{
	public class ServiceHelper : IServiceHelper
	{
		private readonly IPictureparkServiceClient _client;

		public ServiceHelper(IPictureparkServiceClient client)
		{
			_client = client;
		}

		public void EnsureSchemaExists<T>(Action<SchemaDetail> beforeCreateOrUpdateAction, bool updateSchema)
		{
			// Ensure that schema exists in PCP
			var schemas = _client.Schemas.GenerateSchemaFromPOCO(typeof(T), new List<SchemaDetail> { });

			foreach (var schema in schemas)
			{
				beforeCreateOrUpdateAction?.Invoke(schema);

				if (!updateSchema)
				{
					if (!_client.Schemas.Exists(schema.Id))
						_client.Schemas.Create(schema, true);
				}
				else
				{
					_client.Schemas.CreateOrUpdate(schema, true);
				}
			}
		}
	}
}
