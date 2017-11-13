using Picturepark.SDK.V1.Contract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Contract.Extensions;
using Picturepark.SDK.V1.Tests.Fixtures;
using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Tests
{
	public class DocumentHistoryTests : IClassFixture<SDKClientFixture>
	{
		private readonly SDKClientFixture _fixture;
		private readonly PictureparkClient _client;

		public DocumentHistoryTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldSearch()
		{
			/// Arrange
			var request = new DocumentHistorySearchRequest
			{
				Start = 0,
				Limit = 10,
			};

			/// Act
			var result = await _client.DocumentHistory.SearchAsync(request);

			/// Assert
			Assert.True(result.Results.Any());
		}

		[Fact]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGet()
		{
			/// Arrange
			var documentId = _fixture.GetRandomContentId(".jpg", 20);

			/// Act
			var result = await _client.DocumentHistory.GetAsync(documentId);

			/// Assert
			Assert.True(result.DocumentId == documentId);
		}

		[Fact(Skip = "TODO")]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetDifferenceOfContentChange()
		{
			string contentId = _fixture.GetRandomContentId(".jpg", 20);
			var content = await _client.Contents.GetAsync(contentId);

			var updateRequest = new ContentsMetadataUpdateRequest
			{
				ContentIds = new List<string> { content.Id },
				ChangeCommands = new List<MetadataValuesChangeCommandBase>
				{
					new MetadataValuesSchemaUpsertCommand
					{
						SchemaId = "Drive",
						Value = new DataDictionary
						{
							{ "Location", "testlocation" }
						}
					}
				}
			};

			// TODO: Create ContentHelper to update and wait with one call
			var updateResult = await _client.Contents.UpdateMetadataManyAsync(updateRequest);
			var waitResult = await updateResult.WaitForMetadataAsync(_client.BusinessProcesses);
			Assert.True(waitResult.HasStateHit);

			// Refetch content and compare versions
			var updatedContent = await _client.Contents.GetAsync(contentId);
			Assert.NotEqual(1/*updatedContent.Version*/, 0);

			DocumentHistoryDifference result = await _client.DocumentHistory.GetDifferenceLatestAsync(contentId, 1);
		}

		[Fact]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetDifferenceWithLatestVersion()
		{
			/// Arrange
			string documentId = _fixture.GetRandomContentId(".jpg", 20);
			long oldVersionId = 1;

			/// Act
			var difference = await _client.DocumentHistory.GetDifferenceLatestAsync(documentId, oldVersionId);

			/// Assert
			Assert.Equal(difference.OldDocumentVersion, difference.NewDocumentVersion); // no new version
		}

		[Fact(Skip = "Cannot change metadata (NPE)/unable to create a new history version.")]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetDifferenceOfTwoVersions()
		{
			/// Arrange
			string documentId = _fixture.GetRandomContentId(".jpg", 20);
			long oldVersionId = 1;
			long newVersionId = 2;

			/// Act
			var content = await _client.Contents.GetAsync(documentId);

			var tuple = content.Metadata.First();
			var value = (JObject)tuple.Value;
			value[value.Properties().First().Name] = "foo bar";
			content.Metadata[tuple.Key] = tuple.Value;

			var request = new UpdateContentMetadataRequest
			{
				Id = documentId,
				SchemaIds = content.Metadata.Keys.ToList(),
				Metadata = content.Metadata
			};

			await _client.Contents.UpdateMetadataAsync(documentId, request, true);
			var difference = await _client.DocumentHistory.GetDifferenceAsync(documentId, oldVersionId, newVersionId);
		}

		[Fact]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetVersion()
		{
			string documentId = _fixture.GetRandomContentId(".jpg", 20);
			string versionId = "1";

			DocumentHistory result = await _client.DocumentHistory.GetVersionAsync(documentId, versionId);
		}
	}
}
