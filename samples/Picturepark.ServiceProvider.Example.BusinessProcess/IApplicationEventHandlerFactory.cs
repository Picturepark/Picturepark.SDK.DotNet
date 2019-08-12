using Picturepark.SDK.V1.Contract;

namespace Picturepark.ServiceProvider.Example.BusinessProcess
{
    public interface IApplicationEventHandlerFactory
    {
        IApplicationEventHandler Get(ApplicationEvent applicationEvent);
    }
}