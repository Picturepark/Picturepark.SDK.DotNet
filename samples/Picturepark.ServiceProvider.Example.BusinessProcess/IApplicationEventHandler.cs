using Picturepark.SDK.V1.Contract;

namespace Picturepark.ServiceProvider.Example.BusinessProcess
{
    public interface IApplicationEventHandler
    {
        bool Accept(ApplicationEvent applicationEvent);

        void Handle(ApplicationEvent applicationEvent);
    }
}