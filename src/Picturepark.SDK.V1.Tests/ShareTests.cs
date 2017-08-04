using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Extensions;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests
{
	public class ShareTests : IClassFixture<SDKClientFixture>
	{
		private SDKClientFixture _fixture;
		private PictureparkClient _client;

		public ShareTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "Shares")]
		public async Task ShouldAggregate()
		{
			// Create share
			await ShouldCreateEmbedShare();

			var request = new ShareAggregationRequest()
			{
				SearchString = string.Empty,
				Aggregators = new List<AggregatorBase>
				{
					new TermsAggregator
					{
						Field = PropertyHelper.GetName<ShareBase>(i => i.EntityType),
						Size = 10,
						Name = "EntityType"
					}
				}
			};
			var result = await _client.Shares.AggregateAsync(request);

			var aggregation = result.GetByName("EntityType");
			Assert.NotNull(aggregation);

			Assert.True(aggregation.AggregationResultItems.Count >= 1);
		}

		[Fact]
		[Trait("Stack", "Shares")]
		public async Task ShouldCreateBasicShare()
		{
			var outputFormatIds = new List<string>() { "Original" };

			var shareContentItems = new List<ShareContent>()
			{
				new ShareContent() { ContentId = _fixture.GetRandomContentId(string.Empty, 30), OutputFormatIds = outputFormatIds },
				new ShareContent() { ContentId = _fixture.GetRandomContentId(string.Empty, 30), OutputFormatIds = outputFormatIds }
			};

			var recipients = new List<UserEmail>()
			{
				_fixture.Configuration.EmailRecipient
			};

			var request = new ShareBasicCreateRequest()
			{
				Contents = shareContentItems,
				Description = "Description of Basic share aaa",
				ExpirationDate = new DateTime(2020, 12, 31),
				Name = "Basic share aaa",
				RecipientsEmail = recipients
			};

			var result = await _client.Shares.CreateAsync(request);
		}

		[Fact]
		[Trait("Stack", "Shares")]
		public async Task ShouldCreateBasicShareWithWrongContentsAndFail()
		{
			var outputFormatIds = new List<string>() { "Original" };

			var shareContentItems = new List<ShareContent>()
			{
				new ShareContent() { ContentId = "NonExistingId1", OutputFormatIds = outputFormatIds },
				new ShareContent() { ContentId = "NonExistingId2", OutputFormatIds = outputFormatIds }
			};

			var recipients = new List<UserEmail>()
			{
				_fixture.Configuration.EmailRecipient
			};

			var request = new ShareBasicCreateRequest()
			{
				Contents = shareContentItems,
				Description = "Description of share with wrong content ids",
				ExpirationDate = new DateTime(2020, 12, 31),
				Name = "Share with wrong content ids",
				RecipientsEmail = recipients
			};

			await Assert.ThrowsAsync<ContentNotFoundException>(async () =>
			{
				var result = await _client.Shares.CreateAsync(request);
			});
		}

		[Fact]
		[Trait("Stack", "Shares")]
		public async Task ShouldCreateEmbedShare()
		{
			var outputFormatIds = new List<string>() { "Original" };

			var shareContentItems = new List<ShareContent>
			{
				new ShareContent() { ContentId = _fixture.GetRandomContentId(string.Empty, 30), OutputFormatIds = outputFormatIds },
				new ShareContent() { ContentId = _fixture.GetRandomContentId(string.Empty, 30), OutputFormatIds = outputFormatIds }
			};

			var request = new ShareEmbedCreateRequest()
			{
				Contents = shareContentItems,
				Description = "Description of Embed share bbb",
				ExpirationDate = new DateTime(2020, 12, 31),
				Name = "Embed share bbb"
			};

			var result = await _client.Shares.CreateAsync(request);
		}

		[Fact]
		[Trait("Stack", "Shares")]
		public async Task ShouldGetBasicShare()
		{
			string shareId = _fixture.GetRandomShareId(EntityType.BasicShare, 20);
			var result = await _client.Shares.GetAsync(shareId);
		}

		[Fact]
		[Trait("Stack", "Shares")]
		public async Task ShouldGetEmbedShare()
		{
			string shareId = _fixture.GetRandomShareId(EntityType.EmbedShare, 200);
			var result = await _client.Shares.GetAsync(shareId);
		}

		[Fact]
		[Trait("Stack", "Shares")]
		public async Task ShouldSearch()
		{
			var entityType = EntityType.BasicShare;

			var shares = new List<ShareBase>();

			var request = new ContentSearchRequest() { Start = 1, Limit = 100 };
			request.Filter = new TermFilter() { Field = "EntityType" };

			BaseResultOfShareBase result = await _client.Shares.SearchAsync(request);

			foreach (var item in result.Results)
			{
				if (item.EntityType == entityType)
					shares.Add(item);
			}
		}
	}
}
