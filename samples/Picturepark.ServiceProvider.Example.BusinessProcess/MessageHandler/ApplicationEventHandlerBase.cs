using Picturepark.SDK.V1.Contract;

namespace Picturepark.ServiceProvider.Example.BusinessProcess.MessageHandler
{
    internal abstract class ApplicationEventHandlerBase<T> : IApplicationEventHandler where T : ApplicationEvent
    {
        public bool Accept(ApplicationEvent applicationEvent)
        {
            return applicationEvent is T;
        }

        public void Handle(ApplicationEvent applicationEvent)
        {
            Handle((T)applicationEvent);
        }

        protected abstract void Handle(T applicationEvent);
    }
}