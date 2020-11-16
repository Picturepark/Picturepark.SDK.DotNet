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
    [Trait("Stack", "Template")]
    public class TemplateTests : IClassFixture<ClientFixture>
    {
        private readonly ClientFixture _fixture;
        private readonly IPictureparkService _client;

        public TemplateTests(ClientFixture fixture)
        {
            _fixture = fixture;
            _client = fixture.Client;
        }

        [Fact]
        public async Task Should_get_all()
        {
            // Act
            var templates = await _client.Template.GetAllAsync().ConfigureAwait(false);

            // Assert
            templates.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Should_get_single()
        {
            // Arrange
            var templates = await _client.Template.GetAllAsync().ConfigureAwait(false);
            var templateId = templates.First().Id;

            // Act
            var template = await _client.Template.GetAsync(templateId).ConfigureAwait(false);

            // Assert
            template.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_create_custom()
        {
            string templateId = null;

            try
            {
                // Act
                var template = await _client.Template.CreateAsync(
                    new TemplateCreateRequest
                    {
                        Name = "Default",
                        TemplateType = TemplateType.ShareMail,
                        Names = new TranslatedStringDictionary
                        {
                            [_fixture.DefaultLanguage] = "Custom Share Mail"
                        },
                        LanguageCode = _fixture.DefaultLanguage,
                        Values = new List<TemplateValue>
                        {
                            new TemplateValue { MediaType = "text/plain", Text = "Plain template" },
                            new TemplateValue { MediaType = "text/html", Text = "HTML template" },
                        }
                    }).ConfigureAwait(false);

                // Assert
                template.Should().NotBeNull();

                templateId = template.Id;
            }
            finally
            {
                if (!string.IsNullOrEmpty(templateId))
                    await _client.Template.DeleteAsync(templateId).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task Should_update_custom()
        {
            // Arrange
            var template = await CreateCustomTemplate().ConfigureAwait(false);

            // Act
            template.Values.Single(v => v.MediaType == "text/plain").Text = "Updated";

            var updated = await _client.Template.UpdateAsync(
                template.Id,
                new TemplateUpdateRequest
                {
                    Names = template.Names,
                    Values = template.Values
                }).ConfigureAwait(false);

            // Assert
            updated.Values.Single(v => v.MediaType == "text/plain").Text.Should().Be("Updated");
        }

        [Fact]
        public async Task Should_delete_custom()
        {
            // Arrange
            var template = await CreateCustomTemplate().ConfigureAwait(false);

            // Act
            await _client.Template.DeleteAsync(template.Id).ConfigureAwait(false);

            // Assert
            var ex = await Record.ExceptionAsync(() => _client.Template.GetAsync(template.Id)).ConfigureAwait(false);
            ex.Should().BeOfType<TemplateNotFoundException>();
        }

        private async Task<Template> CreateCustomTemplate()
        {
            return await _client.Template.CreateAsync(
                new TemplateCreateRequest
                {
                    Name = $"{Guid.NewGuid():N}",
                    TemplateType = TemplateType.NotificationMailItem,
                    Names = new TranslatedStringDictionary
                    {
                        [_fixture.DefaultLanguage] = "Custom template"
                    },
                    LanguageCode = _fixture.DefaultLanguage,
                    Values = new List<TemplateValue>
                    {
                        new TemplateValue { MediaType = "text/plain", Text = "Plain template" },
                        new TemplateValue { MediaType = "text/html", Text = "HTML template" },
                    }
                }).ConfigureAwait(false);
        }
    }
}