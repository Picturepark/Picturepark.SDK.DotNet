using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "Statistics")]
    public class StatisticsTests : IClassFixture<ClientFixture>, IAsyncLifetime
    {
        private readonly ClientFixture _fixture;
        private string _schemaId;

        public StatisticsTests(ClientFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            _schemaId = nameof(StatisticsTests) + Guid.NewGuid().ToString("N");

            await _fixture.Client.Schema.CreateAsync(new SchemaDetail
            {
                Id = _schemaId,
                Types = new List<SchemaType> { SchemaType.Content },
                Names = new TranslatedStringDictionary
                {
                    [_fixture.CustomerInfo.LanguageConfiguration.DefaultLanguage] = _schemaId
                },
                ViewForAll = true,
                Fields = new List<FieldBase>
                {
                    new FieldString
                    {
                        Id = "testName",
                        Names = new TranslatedStringDictionary
                        {
                            [_fixture.CustomerInfo.LanguageConfiguration.DefaultLanguage] = "testName"
                        }
                    }
                }
            }).ConfigureAwait(false);
        }

        public async Task DisposeAsync()
        {
            await Task.Delay(0).ConfigureAwait(false);
        }

        [Fact]
        public async Task ShouldWriteAndExportStatistics()
        {
            var contentIds = await CreateContents(20).ConfigureAwait(false);

            var fullCurrentHour = new DateTime(DateTime.UtcNow.Ticks / TimeSpan.TicksPerHour * TimeSpan.TicksPerHour, DateTimeKind.Utc);

            var hoursToFill = 3;
            var beginningOfEvents = fullCurrentHour - TimeSpan.FromHours(hoursToFill);
            var endOfEvents = fullCurrentHour;

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

            csvRecords.Should().HaveCount(1 + (contentIds.Count * hoursToFill));

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
                totalDownloadsForHour.Should().Be(60 * downloadsPerContentPerMinute);
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

        private async Task<IReadOnlyCollection<string>> CreateContents(int numContents, [CallerMemberName] string testName = null)
        {
            var createRequests = Enumerable.Range(0, numContents).Select(i => new ContentCreateRequest
            {
                ContentSchemaId = _schemaId,
                Content = new { testName }
            });

            var result = await _fixture.Client.Content.CreateManyAsync(new ContentCreateManyRequest { Items = createRequests.ToList() }).ConfigureAwait(false);
            var detail = await result.FetchDetail().ConfigureAwait(false);
            return detail.SucceededIds;
        }
    }
}