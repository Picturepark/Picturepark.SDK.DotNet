using System.Collections.Concurrent;

namespace Picturepark.ServiceProvider.Example.BusinessProcess.BusinessProcess
{
    internal class BusinessProcessCancellationManager : IBusinessProcessCancellationManager
    {
        private readonly ConcurrentDictionary<string, bool> _cancellationState = new ConcurrentDictionary<string, bool>();

        public void MarkToBeCancelled(string businessProcessId)
        {
            _cancellationState.AddOrUpdate(businessProcessId, true, (_, state) => state);
        }

        public bool IsCancelled(string businessProcessId)
        {
            return _cancellationState.TryGetValue(businessProcessId, out var cancelled) && cancelled;
        }
    }
}