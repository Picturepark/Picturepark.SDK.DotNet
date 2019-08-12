using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Picturepark.SDK.V1.Tests.FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
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

            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();
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

            result.ToList().ForEach(outputFormatDetail =>
                {
                    outputFormatDetail.Audit.CreatedByUser.Should().BeResolved();
                    outputFormatDetail.Audit.ModifiedByUser.Should().BeResolved();
                }
            );
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
            var response = await _client.OutputFormat.UpdateAsync(outputFormat.Id, update).ConfigureAwait(false);

            var result = await response.FetchResult().ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Format.As<JpegFormat>().IsProgressive.Should().BeTrue();
            result.SourceOutputFormats.Audio.Should().Be("Preview");

            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();

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
            var response = await _client.OutputFormat.UpdateManyAsync(new OutputFormatUpdateManyRequest { Items = updateRequests }).ConfigureAwait(false);
            var detail = await response.FetchDetail().ConfigureAwait(false);

            // Assert
            detail.Should().NotBeNull();
            detail.SucceededIds.Should().HaveSameCount(outputFormats);
            detail.FailedIds.Should().BeEmpty();

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
                    Ids = toDeleteIds
                }).ConfigureAwait(false);

            // Assert
            var verifyDeletedOutputFormats = await _client.OutputFormat.GetManyAsync(toDeleteIds).ConfigureAwait(false);
            verifyDeletedOutputFormats.Should().BeEmpty();

            var verifyOutputFormatsStayed = await _client.OutputFormat.GetManyAsync(shouldStayIds).ConfigureAwait(false);
            verifyOutputFormatsStayed.Select(s => s.Id).Should().BeEquivalentTo(shouldStayIds);
        }

        [Fact]
        [Trait("Stack", "OutputFormats")]
        public async Task ShouldCreateSingleOutputFormat()
        {
            // Arrange
            var guid = System.Guid.NewGuid().ToString("N");

            // Act
            var outputFormat = new OutputFormat
            {
                Id = $"OF-test-{guid}",
                Names = new TranslatedStringDictionary
                    {
                        { "en", $"OF_test_{guid}" }
                    },
                Dynamic = true,
                Format = new JpegFormat
                {
                    Quality = 95
                },
                SourceOutputFormats = new SourceOutputFormats
                {
                    Image = "Original",
                    Video = "VideoPreview",
                    Document = "DocumentPreview",
                    Audio = "AudioPreview"
                }
            };
            var result = await _client.OutputFormat.CreateAsync(outputFormat).ConfigureAwait(false);

            // Assert
            result.Should().NotBeNull();
            result.Audit.CreatedByUser.Should().BeResolved();
            result.Audit.ModifiedByUser.Should().BeResolved();
        }
    }
}