using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
	public class SDKClientFixture : IDisposable
	{
		private readonly PictureparkClient _client;
		private readonly Configuration _configuration;

		public SDKClientFixture()
		{
			var configurationJson = File.ReadAllText("Configuration.json");
			_configuration = JsonConvert.DeserializeObject<Configuration>(configurationJson);

			var authClient = new UsernamePasswordAuthClient(_configuration.ApiBaseUrl, _configuration.ApiEmail, _configuration.ApiPassword);
			_client = new PictureparkClient(_configuration.ApiBaseUrl, authClient);

			// _client.SetExceptionHandler((ex, translatedMessage) =>
			// {
			// 	throw ex;
			// });
		}

		public Configuration Configuration => _configuration;

		public PictureparkClient Client => _client;

		public Contract.AssetSearchResult GetRandomAssets(string searchString, int limit)
		{
			return RandomHelper.GetRandomAssets(_client, searchString, limit);
		}

		public string GetRandomAssetId(string searchString, int limit)
		{
			return RandomHelper.GetRandomAssetId(_client, searchString, limit);
		}

		public string GetRandomAssetPermissionSetId(int limit)
		{
			return RandomHelper.GetRandomAssetPermissionSetId(_client, limit);
		}

		public string GetRandomBatchTransferId(TransferState? transferState, int limit)
		{
			return RandomHelper.GetRandomBatchTransferId(_client, transferState, limit);
		}

		public string GetRandomFileTransferId(int limit)
		{
			return RandomHelper.GetRandomFileTransferId(_client, limit);
		}

		public string GetRandomMetadataPermissionSetId(int limit)
		{
			return RandomHelper.GetRandomMetadataPermissionSetId(_client, limit);
		}

		public string GetRandomMetadataSchemaId(int limit)
		{
			return RandomHelper.GetRandomMetadataSchemaId(_client, limit);
		}

		public string GetRandomObjectId(string metadataSchemaId, int limit)
		{
			return RandomHelper.GetRandomObjectId(_client, metadataSchemaId, limit);
		}

		public string GetRandomShareId(EntityType entityType, int limit)
		{
			return RandomHelper.GetRandomShareId(_client, entityType, limit);
		}

		public void Dispose()
		{
			_client.Dispose();
		}
	}
}
