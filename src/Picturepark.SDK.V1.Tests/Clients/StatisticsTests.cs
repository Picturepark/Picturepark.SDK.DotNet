using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "Statistics")]
    public class StatisticsTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;

        public StatisticsTests(ClientFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ShouldWriteAndExportStatistics()
        {
            var contentSearchResult = await _fixture.GetRandomContentsAsync(string.Empty, limit: 20).ConfigureAwait(false);
            var contentIds = contentSearchResult.Results.Select(c => c.Id).ToArray();

            var fullCurrentHour = new DateTime(DateTime.UtcNow.Ticks / TimeSpan.TicksPerHour * TimeSpan.TicksPerHour, DateTimeKind.Utc);

            var hoursToFill = 3;
            var beginningOfEvents = fullCurrentHour - TimeSpan.FromHours(new Random().Next(48, 240));
            var endOfEvents = beginningOfEvents + TimeSpan.FromHours(hoursToFill);

            var downloadsPerContentPerMinute = 20;

            var everyMinute = Intervals(beginningOfEvents, TimeSpan.FromMinutes(1), endOfEvents);
            var everyContentEveryMinute = everyMinute.SelectMany(timestamp => contentIds.Select(contentId => (contentId, timestamp)));

            var events = everyContentEveryMinute.Select(contentAndTime => new AddContentEventsRequestItem
            {
                Timestamp = contentAndTime.timestamp,
                ContentId = contentAndTime.contentId,
                Statistics = new ContentStatisticsDataEditable
                {
                    Downloads = new ContentDownloadsEditable
                    {
                        Total = downloadsPerContentPerMinute,
                        Share = 2,
                        Embed = 1
                    }
                }
            });

            var writeRequest = new AddContentEventsRequest { Events = events.ToArray() };

            var writeProcess = await _fixture.Client.Statistics.AddContentEventsAsync(writeRequest).ConfigureAwait(false);
            await AwaitProcessAndEnsureSuccess(writeProcess).ConfigureAwait(false);

            var exportRequest = new ExportContentStatisticsRequest
            {
                After = beginningOfEvents,
                Before = endOfEvents,
                Interval = TimeSpan.FromHours(1), // default, minimum
                Filter = new ContentFilterRequest
                {
                    Filter = new OrFilter
                    {
                        Filters = contentIds.Select(FilterForContentId).ToList()
                    }
                },
                AggregateApiClients = false
            };
            var exportProcess = await _fixture.Client.Statistics.ExportContentStatisticsAsync(exportRequest).ConfigureAwait(false);

            await AwaitProcessAndEnsureSuccess(exportProcess).ConfigureAwait(false);

            // Use referenceId of export BusinessProcess to resolve download Url
            var downloadLink = await _fixture.Client.Statistics.ResolveDownloadLinkAsync(exportProcess.ReferenceId).ConfigureAwait(false);

            var csvContent = await Download(downloadLink).ConfigureAwait(false);
            var csvRecords = csvContent.Split(new[] { "\r\n" }, StringSplitOptions.None);

            csvRecords.Should().HaveCount(1 + (contentIds.Length * hoursToFill));

            var totalDownloadsFieldIndex = exportRequest.AggregateApiClients ? 2 : 3;

            // skip the header
            foreach (var csvRecord in csvRecords.Skip(1))
            {
                var fields = csvRecord.Split(';');

                var timeStamp = DateTime.Parse(fields[0]);
                var contentId = fields[1];
                var totalDownloadsForHour = int.Parse(fields[totalDownloadsFieldIndex]);

                timeStamp.ToUniversalTime().Should().BeOnOrAfter(beginningOfEvents).And.BeBefore(endOfEvents);
                contentId.Should().BeOneOf(contentIds);
                totalDownloadsForHour.Should().BeGreaterOrEqualTo(60 * downloadsPerContentPerMinute);
            }
        }

        private static async Task<string> Download(DownloadLink downloadLink)
        {
            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetAsync(downloadLink.DownloadUrl).ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        private static IEnumerable<DateTime> Intervals(DateTime begin, TimeSpan intervalSize, DateTime endExclusive)
        {
            for (var current = begin; current < endExclusive; current += intervalSize)
                yield return current;
        }

        private static FilterBase FilterForContentId(string contentId) =>
            FilterBase.FromExpression<Content>(c => c.Id, contentId);

        private async Task AwaitProcessAndEnsureSuccess(BusinessProcess businessProcess)
        {
            var result = await _fixture.Client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id).ConfigureAwait(false);
            result.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);
        }
    }
}