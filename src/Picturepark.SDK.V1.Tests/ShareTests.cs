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
						Field = PropertyHelper.GetName<ShareViewItem>(i => i.EntityType),
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

			var request = new BasicShareCreateItem()
			{
				Contents = shareContentItems,
				Description = "Description of Basic share aaa",
				ExpirationDate = new DateTime(2020, 12, 31),
				Name = "Basic share aaa",
				RecipientsEmail = recipients
			};

			var result = await _client.Shares.CreateBasicShareAsync(request);
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

			var request = new EmbedShareCreateItem()
			{
				ShareContentItems = shareContentItems,
				Description = "Description of Embed share bbb",
				ExpirationDate = new DateTime(2020, 12, 31),
				Name = "Embed share bbb"
			};

			var result = await _client.Shares.CreateEmbedShareAsync(request);
		}

		[Fact]
		[Trait("Stack", "Shares")]
		public async Task ShouldGetBasicShare()
		{
			string shareId = _fixture.GetRandomShareId(EntityType.BasicShare, 20);
			var result = await _client.Shares.GetBasicShareAsync(shareId);
		}

		[Fact]
		[Trait("Stack", "Shares")]
		public async Task ShouldGetEmbedShare()
		{
			string shareId = _fixture.GetRandomShareId(EntityType.EmbedShare, 200);
			var result = await _client.Shares.GetEmbedShareAsync(shareId);
		}

		[Fact]
		[Trait("Stack", "Shares")]
		public async Task ShouldSearch()
		{
			var entityType = EntityType.BasicShare;

			var shareViewItems = new List<ShareViewItem>();

			var request = new ContentSearchRequest() { Start = 1, Limit = 100 };
			request.Filter = new TermFilter() { Field = "EntityType" };

			BaseResultOfShareViewItem result = await _client.Shares.SearchAsync(request);

			foreach (var item in result.Results)
			{
				if (item.EntityType == entityType)
					shareViewItems.Add(item);
			}
		}
	}
}
