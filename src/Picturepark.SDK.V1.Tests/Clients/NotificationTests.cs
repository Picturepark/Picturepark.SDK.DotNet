using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "Notifications")]
    public class NotificationTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public NotificationTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        [Fact]
        public async Task ShouldSearch()
        {
            // Arrange
            var shareId = await CreateShareForNotification().ConfigureAwait(false);

            // Act
            var result = await _client.Notification.SearchAsync(
                new NotificationSearchRequest
                {
                    Sort = new List<SortInfo>
                    {
                        new SortInfo { Field = "audit.creationDate", Direction = SortDirection.Desc }
                    },
                    Limit = 500
                }).ConfigureAwait(false);

            // Assert
            result.Results.Should().Contain(r => r.ReferenceDocType == "ShareDoc" && r.ReferenceId == shareId);
        }

        [Fact]
        public async Task ShouldGet()
        {
            // Arrange
            var notificationId = await GetNotificationId().ConfigureAwait(false);

            // Act
            var notification = await _client.Notification.GetAsync(notificationId).ConfigureAwait(false);

            // Assert
            notification.TitleCode.Should().Be(TitleCode.ShareNewShareTitle);
            notification.State.Should().Be(NotificationState.Unread);
        }

        [Fact]
        public async Task ShouldMarkAsRead()
        {
            // Arrange
            var notificationId = await GetNotificationId().ConfigureAwait(false);

            // Act
            await _client.Notification.MarkAsReadAsync(notificationId).ConfigureAwait(false);

            // Assert
            var notification = await _client.Notification.GetAsync(notificationId).ConfigureAwait(false);
            notification.State.Should().Be(NotificationState.Read);
        }

        [Fact]
        public async Task ShouldMarkAllAsRead()
        {
            // Arrange
            var notificationId1 = await GetNotificationId().ConfigureAwait(false);
            var notificationId2 = await GetNotificationId().ConfigureAwait(false);

            // Act
            await _client.Notification.MarkAllAsReadAsync().ConfigureAwait(false);

            // Assert
            var notification = await _client.Notification.GetAsync(notificationId1).ConfigureAwait(false);
            notification.State.Should().Be(NotificationState.Read);

            notification = await _client.Notification.GetAsync(notificationId2).ConfigureAwait(false);
            notification.State.Should().Be(NotificationState.Read);
        }

        [Fact]
        public async Task ShouldGetDigestConfiguration()
        {
            // Act
            var configuration = await _client.Notification.GetDigestConfigurationAsync().ConfigureAwait(false);

            // Assert
            configuration.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldPutDigestConfiguration()
        {
            // Arrange
            var notificationId = $"{Guid.NewGuid():N}";

            var businessProcess = await _client.BusinessRule.UpdateConfigurationAsync(
                new BusinessRuleConfigurationUpdateRequest
                {
                    Notifications = new List<BusinessRuleNotification>
                    {
                        new BusinessRuleNotification
                        {
                            Id = notificationId,
                            Title = new TranslatedStringDictionary { [_fixture.DefaultLanguage] = "Title" },
                            Message = new TranslatedStringDictionary { [_fixture.DefaultLanguage] = "Message" }
                        }
                    }
                }).ConfigureAwait(false);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id).ConfigureAwait(false);

            // Act
            var configuration = await _client.Notification.PutDigestConfigurationAsync(
                new NotificationDigestConfiguration
                {
                    Interval = NotificationDigestInterval.Hourly,
                    DigestDisabledByDefault = true,
                    Items = new List<NotificationDigestConfigurationItem>
                    {
                        new NotificationDigestConfigurationItem
                        {
                            DigestEnabled = true,
                            NotificationId = notificationId
                        }
                    }
                }).ConfigureAwait(false);

            // Assert
            configuration.Interval.Should().Be(NotificationDigestInterval.Hourly);
            configuration.DigestDisabledByDefault.Should().BeTrue();

            var item = configuration.Items.Should().HaveCount(1).And.ContainSingle().Which;
            item.NotificationId.Should().Be(notificationId);
            item.DigestEnabled.Should().BeTrue();
        }

        private async Task<string> GetNotificationId()
        {
            var shareId = await CreateShareForNotification().ConfigureAwait(false);

            var result = await _client.Notification.SearchAsync(
                new NotificationSearchRequest
                {
                    Sort = new List<SortInfo>
                    {
                        new SortInfo { Field = "audit.creationDate", Direction = SortDirection.Desc }
                    },
                    Limit = 500
                }).ConfigureAwait(false);

            return result.Results.Single(r => r.ReferenceDocType == "ShareDoc" && r.ReferenceId == shareId).Id;
        }

        private async Task<string> CreateShareForNotification()
        {
            var businessProcess = await _client.Share.CreateAsync(
                new ShareBasicCreateRequest
                {
                    Name = $"{Guid.NewGuid():N}",
                    SuppressNotifications = false,
                    LanguageCode = "en"
                }).ConfigureAwait(false);

            var result = await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id).ConfigureAwait(false);
            result.LifeCycleHit.Should().Be(BusinessProcessLifeCycle.Succeeded);
            return businessProcess.ReferenceId;
        }
    }
}