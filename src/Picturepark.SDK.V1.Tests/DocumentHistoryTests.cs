using Picturepark.SDK.V1.Contract;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Picturepark.SDK.V1.Contract.Extensions;
using Picturepark.SDK.V1.Tests.Fixtures;

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

		[Fact]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetVersion()
		{
			/// Arrange
			string documentId = _fixture.GetRandomContentId(".jpg", 20);
			string versionId = "1";

			/// Act
			var result = await _client.DocumentHistory.GetVersionAsync(documentId, versionId);

			/// Assert
			Assert.Equal(versionId, result.DocumentVersion.ToString());
		}

		[Fact]
		[Trait("Stack", "DocumentHistory")]
		public async Task ShouldGetDifferenceOfContentChange()
		{
			/// Arrange
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
			var waitResult = await updateResult.WaitForCompletionAsync(_client.BusinessProcesses);

			// Refetch content and compare versions
			var updatedContent = await _client.DocumentHistory.GetAsync(contentId);

			/// Act
			var difference = await _client.DocumentHistory.GetDifferenceLatestAsync(contentId, 1);

			/// Assert
			Assert.True(waitResult.HasLifeCycleHit);
			Assert.NotEqual(updatedContent.DocumentVersion, 0);
			Assert.True(difference.NewValues.ToString().Contains(@"""location"": ""testlocation"""));		}

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
			Assert.True(difference.OldDocumentVersion <= difference.NewDocumentVersion);
		}
	}
}
