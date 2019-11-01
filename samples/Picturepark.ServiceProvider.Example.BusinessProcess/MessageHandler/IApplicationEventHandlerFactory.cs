using Picturepark.SDK.V1.Contract;

namespace Picturepark.ServiceProvider.Example.BusinessProcess.MessageHandler
{
    public interface IApplicationEventHandlerFactory
    {
        IApplicationEventHandler Get(ApplicationEvent applicationEvent);
    }
}