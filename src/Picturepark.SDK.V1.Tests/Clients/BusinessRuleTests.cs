using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.Tests.Clients
{
    [Trait("Stack", "BusinessRule")]
    public class BusinessRuleTests : IClassFixture<ClientFixture>
    {
        private readonly IPictureparkService _client;

        public BusinessRuleTests(ClientFixture fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
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
                        TriggerPoints = new[]
                        {
                            new BusinessRuleTriggerPoint
                            {
                                DocumentType = BusinessRuleTriggerDocType.Content,
                                ExecutionScope = BusinessRuleExecutionScope.MainDoc,
                                Action = BusinessRuleTriggerAction.Create
                            }
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

            var businessProcess = await _client.BusinessRule.UpdateConfigurationAsync(request);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            // retrieve config again
            var storedConfiguration = await _client.BusinessRule.GetConfigurationAsync();
            storedConfiguration.DisableRuleEngine.Should().BeTrue();
            storedConfiguration.Rules.Should().HaveCount(1);
        }

        [Fact]
        public async Task Should_retrieve_trace_log()
        {
            // Arrange
            var contentSchemaId = $"ContentType{Guid.NewGuid():N}";

            await _client.Schema.CreateAsync(
                new SchemaCreateRequest
                {
                    Id = contentSchemaId,
                    Names = new TranslatedStringDictionary { { "en", contentSchemaId } },
                    ViewForAll = true,
                    Types = new List<SchemaType> { SchemaType.Content },
                    Fields = new List<FieldBase>
                    {
                        new FieldString { Id = "title", Names = new TranslatedStringDictionary { { "en", "title" } } }
                    }
                });

            var ruleId = $"{Guid.NewGuid():N}";
            var request = new BusinessRuleConfigurationUpdateRequest
            {
                DisableRuleEngine = false,
                Rules = new List<BusinessRule>
                {
                    new BusinessRuleConfigurable
                    {
                        EnableTracing = true,
                        Id = ruleId,
                        Names = new TranslatedStringDictionary { { "en", $"{ruleId}_name" } },
                        Condition = new ContentSchemaCondition { SchemaId = contentSchemaId },
                        Actions = new List<BusinessRuleAction> { new ProduceMessageAction() },
                        IsEnabled = true,
                        TriggerPoints = new[]
                        {
                            new BusinessRuleTriggerPoint
                            {
                                DocumentType = BusinessRuleTriggerDocType.Content,
                                Action = BusinessRuleTriggerAction.Create,
                                ExecutionScope = BusinessRuleExecutionScope.MainDoc
                            }
                        }
                    }
                }
            };

            var businessProcess = await _client.BusinessRule.UpdateConfigurationAsync(request);
            await _client.BusinessProcess.WaitForCompletionAsync(businessProcess.Id);

            // Act
            var content = await _client.Content.CreateAsync(new ContentCreateRequest { ContentSchemaId = contentSchemaId, Content = new { title = "test" } })
                ;

            // Assert
            var result = await _client.BusinessRule.SearchTracesAsync(
                new BusinessRuleTraceLogSearchRequest
                {
                    Filter = new AndFilter
                    {
                        Filters = new[]
                        {
                            FilterBase.FromExpression<BusinessRuleTraceLog>(t => t.DocumentType, BusinessRuleTriggerDocType.Content.ToString()),
                            FilterBase.FromExpression<BusinessRuleTraceLog>(t => t.DocumentId, content.Id)
                        }
                    },
                    Aggregators = new List<AggregatorBase>
                    {
                        new TermsAggregator
                        {
                            Name = "ruleIds",
                            Field = nameof(BusinessRuleTraceLog.RuleIds).ToLowerCamelCase()
                        }
                    }
                });

            var traceLog = result.Results.Should().ContainSingle().Which;
            traceLog.RuleIds.Should().HaveCount(1).And.Contain(ruleId);

            var ruleLog = traceLog.Rules.Should().ContainSingle().Which;
            var evaluation = ruleLog.Evaluations.Should().ContainSingle().Which;

            evaluation.Conditions.Should().ContainSingle().Which.Satisfied.Should().BeTrue();

            result.AggregationResults.Should().ContainSingle(a => a.Name == "ruleIds").Which.AggregationResultItems.Should()
                .ContainSingle(ar => ar.Count == 1 && ar.Name == ruleId);
        }
    }
}