using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    public class BusinessRuleTests : IClassFixture<ClientFixture>
    {
        private readonly IPictureparkService _client;

        public BusinessRuleTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        [Trait("Stack", "BusinessRule")]
        public async Task ShouldStoreAndRetrieveConfiguration()
        {
            // in order not to interfere with other tests, we intentionally disable the rule engine here
            var request = new BusinessRuleConfigurationUpdateRequest
            {
                DisableRuleEngine = true,
                Rules = new List<BusinessRule>
                {
                    new BusinessRuleConfigurable
                    {
                        Id = $"{Guid.NewGuid():N}",
                        Names = new TranslatedStringDictionary
                        {
                            { "en", "A sample rule" }
                        },
                        Description = new TranslatedStringDictionary
                        {
                            { "en", "Assign a permission set upon uploading of any image or video" }
                        },
                        IsEnabled = true,
                        TriggerPoint = new BusinessRuleTriggerPoint
                        {
                            DocumentType = BusinessRuleTriggerDocType.Content,
                            ExecutionScope = BusinessRuleExecutionScope.MainDoc,
                            Action = BusinessRuleTriggerAction.Create
                        },
                        Condition = new OrCondition
                        {
                            Conditions = new List<BusinessRuleCondition>
                            {
                                new ContentSchemaCondition { SchemaId = "ImageMetadata" },
                                new ContentSchemaCondition { SchemaId = "VideoMetadata" }
                            }
                        },
                        Actions = new List<BusinessRuleAction>
                        {
                            new AssignContentPermissionSetsAction
                            {
                                PermissionSetIds = new[] { $"{Guid.NewGuid():N}" }
                            }
                        }
                    }
                }
            };

            var businessProcess = await _client.BusinessRule.UpdateConfigurationAsync(request).ConfigureAwait(false);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id).ConfigureAwait(false);

            // retrieve config again
            var storedConfiguration = await _client.BusinessRule.GetConfigurationAsync().ConfigureAwait(false);
            storedConfiguration.DisableRuleEngine.Should().BeTrue();
            storedConfiguration.Rules.Should().HaveCount(1);
        }
    }
}