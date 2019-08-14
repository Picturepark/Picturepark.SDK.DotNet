using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Picturepark.SDK.V1.Contract;
using Picturepark.ServiceProvider.Example.BusinessProcess.Config;
using Picturepark.ServiceProvider.Example.BusinessProcess.Util;

namespace Picturepark.ServiceProvider.Example.BusinessProcess.MessageHandler
{
    internal class BusinessRuleFiredEventHandler : ApplicationEventHandlerBase<BusinessRuleFiredEvent>
    {
        private readonly IOptions<SampleConfiguration> _config;
        private readonly ILogger<BusinessRuleFiredEventHandler> _logger;
        private readonly ContentIdQueue _queue;

        public BusinessRuleFiredEventHandler(IOptions<SampleConfiguration> config, ILogger<BusinessRuleFiredEventHandler> logger, ContentIdQueue queue)
        {
            _config = config;
            _logger = logger;
            _queue = queue;
        }

        protected override void Handle(BusinessRuleFiredEvent applicationEvent)
        {
            var triggeredFor = applicationEvent.Details.Where(d => d.DocumentType == "ContentDoc" && d.RuleIds.Contains(_config.Value.TriggeringBusinessRuleId))
                .Select(d => d.DocumentId).ToArray();

            if (triggeredFor.Any())
            {
                _logger.LogInformation($"{triggeredFor.Length} new contents uploaded, will enqueue for batch operation");

                foreach (var contentId in triggeredFor)
                    _queue.Add(contentId);
            }
        }
    }
}