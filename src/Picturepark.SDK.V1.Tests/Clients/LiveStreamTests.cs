﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class LiveStreamTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public LiveStreamTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "LiveStream")]
        public async Task ShouldReturnSearchResultsCorrectly()
        {
            // Arrange
            var time = DateTime.Now;
            await _fixture.Users.Create().ConfigureAwait(false);

            var request = new LiveStreamSearchRequest
            {
                Limit = 10,
                From = time,
                ScopeType = "DocumentChange"
            };

            // Act
            var result = await _client.LiveStream.SearchAsync(request).ConfigureAwait(false);

            // Assert
            result.Results.Should().NotBeEmpty();
        }
    }
}