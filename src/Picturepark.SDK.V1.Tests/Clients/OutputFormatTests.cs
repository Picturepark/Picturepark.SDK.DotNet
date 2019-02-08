using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class OutputFormatTests : IClassFixture<OutputFormatFixture>
    {
        private readonly OutputFormatFixture _fixture;
        private readonly IPictureparkService _client;

        public OutputFormatTests(OutputFormatFixture fixture)
        {
            _fixture = fixture;
            _client = _fixture.Client;
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldCreateOutputFormat()
        {
            // Arrange
            var outputFormat = await _fixture.CreateOutputFormat().ConfigureAwait(false);

            // Act
            var result = await _client.OutputFormat.GetAsync(outputFormat.Id).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(outputFormat.Id);
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldCreateMultipleOutputFormats()
        {
            // Arrange
            var outputFormat = await _fixture.CreateOutputFormats(3).ConfigureAwait(false);
            var outputFormatIds = outputFormat.Select(s => s.Id).ToArray();

            // Act
            var result = await _client.OutputFormat.GetManyAsync(outputFormatIds).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Select(of => of.Id).Should().BeEquivalentTo(outputFormatIds);
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldUpdateOutputFormatCorrectly()
        {
            // Arrange
            var outputFormat = await _fixture.CreateOutputFormat().ConfigureAwait(false);

            var update = new OutputFormatEditable
            {
                Names = outputFormat.Names,
                Format = outputFormat.Format,
                RetentionTime = outputFormat.RetentionTime,
                SourceOutputFormats = outputFormat.SourceOutputFormats
            };
            update.Format.As<JpegFormat>().IsProgressive = true;
            update.SourceOutputFormats.Audio = "Preview";

            // Act
            var result = await _client.OutputFormat.UpdateAsync(outputFormat.Id, update).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Format.As<JpegFormat>().IsProgressive.Should().BeTrue();
            result.SourceOutputFormats.Audio.Should().Be("Preview");

            var verifyOutputFormat = await _client.OutputFormat.GetAsync(outputFormat.Id).ConfigureAwait(false);

            verifyOutputFormat.Format.As<JpegFormat>().IsProgressive.Should().BeTrue();
            verifyOutputFormat.SourceOutputFormats.Audio.Should().Be("Preview");
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldUpdateMultipleOutputFormatsCorrectly()
        {
            // Arrange
            var outputFormats = await _fixture.CreateOutputFormats(3).ConfigureAwait(false);
            var outputFormatIds = outputFormats.Select(of => of.Id);

            var updateRequests = outputFormats.Select((of, i) =>
            {
                var request = new OutputFormatUpdateManyRequestItem
                {
                    Id = of.Id,
                    Names = of.Names,
                    Format = of.Format,
                    RetentionTime = of.RetentionTime,
                    SourceOutputFormats = of.SourceOutputFormats
                };
                request.Format.As<JpegFormat>().IsProgressive = true;
                request.SourceOutputFormats.Audio = "Preview";
                request.Format.As<JpegFormat>().Quality = i;

                return request;
            }).ToArray();

            // Act
            var result = await _client.OutputFormat.UpdateManyAsync(new OutputFormatUpdateManyRequest { Items = updateRequests }).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Rows.Should().HaveSameCount(outputFormats);
            result.Rows.Should().OnlyContain(r => r.Succeeded);

            var verifyOutputFormats = await _client.OutputFormat.GetManyAsync(outputFormatIds).ConfigureAwait(false);

            for (var j = 0; j < outputFormats.Count; j++)
            {
                var verifyFormat = verifyOutputFormats.Should().ContainSingle(of => of.Id == outputFormats[j].Id).Subject;
                verifyFormat.Format.As<JpegFormat>().IsProgressive.Should().BeTrue();
                verifyFormat.Format.As<JpegFormat>().Quality.Should().Be(j);
                verifyFormat.SourceOutputFormats.Audio.Should().Be("Preview");
            }
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldDeleteOutputFormat()
        {
            // Arrange
            var outputFormat = await _fixture.CreateOutputFormat().ConfigureAwait(false);

            // Act
            await _client.OutputFormat.DeleteAsync(outputFormat.Id).ConfigureAwait(false);

            // Assert
            Action checkIfExists = () => _client.OutputFormat.GetAsync(outputFormat.Id).GetAwaiter().GetResult();

            checkIfExists.Should().Throw<PictureparkNotFoundException>();
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldDeleteMultipleOutputFormats()
        {
            // Arrange
            var permissionSets = await _fixture.CreateOutputFormats(5).ConfigureAwait(false);
            var toDeleteIds = permissionSets.Take(3).Select(s => s.Id).ToArray();
            var shouldStayIds = permissionSets.Skip(3).Select(s => s.Id).ToArray();

            // Act
            await _client.OutputFormat.DeleteManyAsync(
                new OutputFormatDeleteManyRequest
                {
                    Items = toDeleteIds
                }).ConfigureAwait(false);

            // Assert
            var verifyDeletedOutputFormats = await _client.OutputFormat.GetManyAsync(toDeleteIds).ConfigureAwait(false);
            verifyDeletedOutputFormats.Should().BeEmpty();

            var verifyOutputFormatsStayed = await _client.OutputFormat.GetManyAsync(shouldStayIds).ConfigureAwait(false);
            verifyOutputFormatsStayed.Select(s => s.Id).Should().BeEquivalentTo(shouldStayIds);
        }
    }
}