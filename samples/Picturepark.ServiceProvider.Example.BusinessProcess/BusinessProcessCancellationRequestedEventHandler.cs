using Picturepark.SDK.V1.Contract;

namespace Picturepark.ServiceProvider.Example.BusinessProcess
{
    internal class BusinessProcessCancellationRequestedEventHandler  : ApplicationEventHandlerBase<BusinessProcessCancellationRequestedEvent>
    {
        private readonly IBusinessProcessCancellationManager _cancellationManager;

        public BusinessProcessCancellationRequestedEventHandler(IBusinessProcessCancellationManager cancellationManager)
        {
            _cancellationManager = cancellationManager;
        }

        protected override void Handle(BusinessProcessCancellationRequestedEvent applicationEvent)
        {
            var businessProcessId = applicationEvent.BusinessProcessId;
            _cancellationManager.MarkToBeCancelled(businessProcessId);
        }
    }
}