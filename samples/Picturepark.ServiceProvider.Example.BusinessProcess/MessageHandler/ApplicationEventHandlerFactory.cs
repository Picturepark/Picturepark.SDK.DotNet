using System.Collections.Generic;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.ServiceProvider.Example.BusinessProcess.MessageHandler
{
    internal class ApplicationEventHandlerFactory : IApplicationEventHandlerFactory
    {
        private readonly IEnumerable<IApplicationEventHandler> _handlers;

        public ApplicationEventHandlerFactory(IEnumerable<IApplicationEventHandler> handlers)
        {
            _handlers = handlers;
        }

        public IApplicationEventHandler Get(ApplicationEvent applicationEvent)
        {
            foreach (var handler in _handlers)
            {
                if (handler.Accept(applicationEvent))
                    return handler;
            }

            return null;
        }
    }
}