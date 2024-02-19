using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Contracts;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;
using System.Linq;
using FluentAssertions;

#pragma warning disable 1587

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "BusinessProcesses")]
    public class BusinessProcessTests : IClassFixture<ClientFixture>
    {
        private readonly IPictureparkService _client;

        public BusinessProcessTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task ShouldSearchBusinessProcesses()
        {
            // Arrange
            var request = new BusinessProcessSearchRequest
            {
                Limit = 20
            };

            // Act
            var results = await _client.BusinessProcess.SearchAsync(request);

            // Assert
            Assert.NotNull(results);
            Assert.True(results.Results.Any());
        }

        [Fact]
        public async Task ShouldGetBusinessProcessDetails()
        {
            // Arrange

            // 1. Create or update schema
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(BusinessProcessTest));
            await _client.Schema.CreateOrUpdateAsync(schemas.First(), false, TimeSpan.FromMinutes(1));

            // 2. Create list items
            var listItemDetail1 = await _client.ListItem.CreateAsync(new ListItemCreateRequest
            {
                Content = new BusinessProcessTest { Name = "Test1" },
                ContentSchemaId = nameof(BusinessProcessTest)
            });

            var listItemDetail2 = await _client.ListItem.CreateAsync(new ListItemCreateRequest
            {
                Content = new BusinessProcessTest { Name = "Test2" },
                ContentSchemaId = nameof(BusinessProcessTest)
            });

            // 3. Initialize change request
            var updateRequest = new ListItemFieldsBatchUpdateRequest
            {
                ListItemIds = new List<string>
                {
                    listItemDetail1.Id,
                    listItemDetail2.Id
                },
                ChangeCommands = new List<MetadataValuesChangeCommandBase>
                {
                    new MetadataValuesSchemaUpdateCommand
                    {
                        SchemaId = nameof(BusinessProcessTest),
                        Value = new
                        {
                            Description = "TestDescription"
                        }
                    }
                }
            };

            var result = await _client.ListItem.BatchUpdateFieldsByIdsAsync(updateRequest);

            // Act
            var summary = await _client.BusinessProcess.GetSummaryAsync(result.BusinessProcessId) as BusinessProcessSummaryBatchBased;
            var rows = new List<BatchResponseRow>();

            string pageToken = null;
            do
            {
                var page = await _client.BusinessProcess.GetSuccessfulItemsAsync(result.BusinessProcessId, 1, pageToken);
                if (page.Data is BusinessProcessBatchItemBatchResponse batchResponse)
                    rows.AddRange(batchResponse.Items);

                pageToken = page.PageToken;
            }
            while (pageToken != null);

            // Assert
            summary.Should().NotBeNull();
            summary.SucceededItemCount.Should().Be(2);

            rows.Should().HaveCount(2);
        }

        [Fact]
        public async Task Should_create_external_business_process()
        {
            var businessProcess = await CreateBusinessProcess();

            businessProcess.SupportsCancellation.Should().BeFalse();
            businessProcess.CurrentState.Should().Be("Started");
        }

        [Fact]
        public async Task Should_change_business_process_state()
        {
            var businessProcess = await CreateBusinessProcess();

            var updated = await _client.BusinessProcess.ChangeStateAsync(
                businessProcess.Id,
                new BusinessProcessStateChangeRequest
                {
                    State = "Intermediate",
                    LifeCycle = BusinessProcessLifeCycle.InProgress
                });

            updated.CurrentState.Should().Be("Intermediate");
            updated.LifeCycle.Should().Be(BusinessProcessLifeCycle.InProgress);
            updated.StateHistory.Should().Contain(x => x.State == "Intermediate");
        }

        [Fact]
        public async Task Should_post_notification_update()
        {
            var businessProcess = await CreateBusinessProcess();

            // passes when no exception is risen
            await _client.BusinessProcess.UpdateNotificationAsync(
                businessProcess.Id,
                new BusinessProcessNotificationUpdateRequest
                {
                    Title = new TranslatedStringDictionary
                    {
                        { "en", "Title updated" },
                        { "de", "Aktualisierter Titel" }
                    },
                    Message = new TranslatedStringDictionary
                    {
                        { "en", "Message updated" },
                        { "de", "Aktualisierte Nachricht" }
                    },
                    EventType = NotificationEventType.Warning
                });
        }

        [Fact]
        public async Task Should_cancel_running_business_process()
        {
            var businessProcess = await CreateBusinessProcess(supportsCancellation: true);

            await _client.BusinessProcess.CancelAsync(businessProcess.Id);

            businessProcess = await _client.BusinessProcess.GetAsync(businessProcess.Id);
            businessProcess.LifeCycle.Should().Be(BusinessProcessLifeCycle.CancellationInProgress);

            businessProcess = await _client.BusinessProcess.ChangeStateAsync(
                businessProcess.Id,
                new BusinessProcessStateChangeRequest
                {
                    LifeCycle = BusinessProcessLifeCycle.Cancelled,
                    State = "Cancelled"
                });

            businessProcess.CurrentState.Should().Be("Cancelled");
            businessProcess.LifeCycle.Should().Be(BusinessProcessLifeCycle.Cancelled);
        }

        [Fact]
        public async Task Should_get_business_process()
        {
            var businessProcess = await CreateBusinessProcess();

            var retrieved = await _client.BusinessProcess.GetAsync(businessProcess.Id);
            retrieved.Id.Should().Be(businessProcess.Id);
        }

        private async Task<BusinessProcess> CreateBusinessProcess(bool supportsCancellation = false)
        {
            var businessProcess = await _client.BusinessProcess.CreateAsync(
                new BusinessProcessCreateRequest
                {
                    InitialState = "Started",
                    SupportsCancellation = supportsCancellation,
                    Notification = new BusinessProcessNotificationUpdate
                    {
                        EventType = NotificationEventType.InProgress,
                        Message = new TranslatedStringDictionary
                        {
                            { "en", "Message" },
                            { "de", "Nachricht" }
                        },
                        Title = new TranslatedStringDictionary
                        {
                            { "en", "Title" },
                            { "de", "Titel" }
                        }
                    }
                });

            return businessProcess;
        }
    }
}
