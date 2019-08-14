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
            var results = await _client.BusinessProcess.SearchAsync(request).ConfigureAwait(false);

            // Assert
            Assert.NotNull(results);
            Assert.True(results.Results.Any());
        }

        [Fact]
        public async Task ShouldGetBusinessProcessDetails()
        {
            // Arrange

            // 1. Create or update schema
            var schemas = await _client.Schema.GenerateSchemasAsync(typeof(BusinessProcessTest)).ConfigureAwait(false);
            await _client.Schema.CreateOrUpdateAsync(schemas.First(), false, TimeSpan.FromMinutes(1)).ConfigureAwait(false);

            // 2. Create list items
            var listItemDetail1 = await _client.ListItem.CreateAsync(new ListItemCreateRequest
            {
                Content = new BusinessProcessTest { Name = "Test1" },
                ContentSchemaId = nameof(BusinessProcessTest)
            }).ConfigureAwait(false);

            var listItemDetail2 = await _client.ListItem.CreateAsync(new ListItemCreateRequest
            {
                Content = new BusinessProcessTest { Name = "Test2" },
                ContentSchemaId = nameof(BusinessProcessTest)
            }).ConfigureAwait(false);

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
                        Value = new DataDictionary
                        {
                            { "Description", "TestDescription" }
                        }
                    }
                }
            };

            var result = await _client.ListItem.BatchUpdateFieldsByIdsAsync(updateRequest).ConfigureAwait(false);

            // Act
            var details = await _client.BusinessProcess.GetDetailsAsync(result.BusinessProcessId).ConfigureAwait(false);

            // Assert
            Assert.True(details.LifeCycle == BusinessProcessLifeCycle.Succeeded);
        }

        [Fact]
        public async Task Should_create_external_business_process()
        {
            var businessProcess = await CreateBusinessProcess().ConfigureAwait(false);

            businessProcess.SupportsCancellation.Should().BeFalse();
            businessProcess.CurrentState.Should().Be("Started");
        }

        [Fact]
        public async Task Should_change_business_process_state()
        {
            var businessProcess = await CreateBusinessProcess().ConfigureAwait(false);

            var updated = await _client.BusinessProcess.ChangeStateAsync(
                businessProcess.Id,
                new BusinessProcessStateChangeRequest
                {
                    State = "Intermediate",
                    LifeCycle = BusinessProcessLifeCycle.InProgress
                }).ConfigureAwait(false);

            updated.CurrentState.Should().Be("Intermediate");
            updated.LifeCycle.Should().Be(BusinessProcessLifeCycle.InProgress);
            updated.StateHistory.Should().Contain(x => x.State == "Intermediate");
        }

        [Fact]
        public async Task Should_post_notification_update()
        {
            var businessProcess = await CreateBusinessProcess().ConfigureAwait(false);

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
                }).ConfigureAwait(false);
        }

        [Fact]
        public async Task Should_cancel_running_business_process()
        {
            var businessProcess = await CreateBusinessProcess().ConfigureAwait(false);

            await _client.BusinessProcess.CancelAsync(businessProcess.Id).ConfigureAwait(false);

            businessProcess = await _client.BusinessProcess.GetAsync(businessProcess.Id).ConfigureAwait(false);
            businessProcess.LifeCycle.Should().Be(BusinessProcessLifeCycle.CancellationInProgress);

            businessProcess = await _client.BusinessProcess.ChangeStateAsync(
                businessProcess.Id,
                new BusinessProcessStateChangeRequest
                {
                    LifeCycle = BusinessProcessLifeCycle.Cancelled,
                    State = "Cancelled"
                }).ConfigureAwait(false);

            businessProcess.CurrentState.Should().Be("Cancelled");
            businessProcess.LifeCycle.Should().Be(BusinessProcessLifeCycle.Cancelled);
        }

        [Fact]
        public async Task Should_get_business_process()
        {
            var businessProcess = await CreateBusinessProcess().ConfigureAwait(false);

            var retrieved = await _client.BusinessProcess.GetAsync(businessProcess.Id).ConfigureAwait(false);
            retrieved.Id.Should().Be(businessProcess.Id);
        }

        private async Task<BusinessProcess> CreateBusinessProcess()
        {
            var businessProcess = await _client.BusinessProcess.CreateAsync(
                new BusinessProcessCreateRequest
                {
                    InitialState = "Started",
                    SupportsCancellation = false,
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
                }).ConfigureAwait(false);

            return businessProcess;
        }
    }
}
