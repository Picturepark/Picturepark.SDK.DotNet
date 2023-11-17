using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Xunit;
using Picturepark.SDK.V1.Tests.Fixtures;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "Channels")]
    public class ChannelTests : IClassFixture<ChannelFixture>
    {
        private readonly ChannelFixture _fixture;
        private readonly IPictureparkService _client;

        public ChannelTests(ChannelFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        [Fact]
        public async Task ShouldGetChannels()
        {
            // Act
            var channels = await _client.Channel.GetAllAsync();

            // Assert
            channels.Should().NotBeEmpty();
        }

        [Fact]
        public async Task ShouldCreateAndGetChannel()
        {
            // Arrange
            var createdChannel = await _fixture.CreateChannel();

            // Act
            var channel = await _client.Channel.GetAsync(createdChannel.Id);

            // Assert
            channel.Should().NotBeNull();
            channel.Id.Should().Be(createdChannel.Id);
        }

        [Fact]
        public async Task ShouldUpdateChannel()
        {
            // Arrange
            var createdChannel = await _fixture.CreateChannel();

            var updateRequest = new ChannelUpdateRequest
            {
                Names = createdChannel.Names,
                ViewForAll = true,
                SearchIndexId = createdChannel.SearchIndexId
            };

            // Act
            var updatedChannel = await _client.Channel.UpdateAsync(createdChannel.Id, updateRequest);

            // Assert
            updatedChannel.Should().NotBeNull();
            updatedChannel.Id.Should().Be(createdChannel.Id);
            updatedChannel.Names.Should().BeEquivalentTo(createdChannel.Names);
            updatedChannel.ViewForAll.Should().Be(true);
        }

        [Fact]
        public async Task ShouldDeleteChannel()
        {
            // Arrange
            var createdChannel = await _fixture.CreateChannel();

            // Act
            await _client.Channel.DeleteAsync(createdChannel.Id);

            // Assert
            await Assert.ThrowsAsync<ChannelsNotFoundException>(async () => await _client.Channel.GetAsync(createdChannel.Id));
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGetAggregationFields()
        {
            // Act
            var fieldsInfo = await _client.Channel.GetAggregationFieldsAsync();

            // Assert
            fieldsInfo.Count(f => !f.Static).Should().BePositive();
            fieldsInfo.Count(f => f.Static).Should().BePositive();
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGetFilterFields()
        {
            // Act
            var fieldsInfo = await _client.Channel.GetFilterFieldsAsync();

            // Assert
            fieldsInfo.Count(f => !f.Static).Should().BePositive();
            fieldsInfo.Count(f => f.Static).Should().BePositive();
        }

        [Fact]
        [Trait("Stack", "Schema")]
        public async Task ShouldGetSortFields()
        {
            // Act
            var sortFields = await _client.Channel.GetSortFieldsAsync();

            // Assert
            sortFields.Count(f => !f.Static).Should().BePositive();
            sortFields.Count(f => f.Static).Should().BePositive();
        }
    }
}
