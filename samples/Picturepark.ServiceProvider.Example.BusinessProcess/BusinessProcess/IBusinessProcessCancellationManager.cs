namespace Picturepark.ServiceProvider.Example.BusinessProcess.BusinessProcess
{
    internal interface IBusinessProcessCancellationManager
    {
        void MarkToBeCancelled(string businessProcessId);

        bool IsCancelled(string businessProcessId);
    }
}