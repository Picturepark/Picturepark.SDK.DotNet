namespace Picturepark.ServiceProvider.Example.BusinessProcess
{
    internal interface IBusinessProcessCancellationManager
    {
        void MarkToBeCancelled(string businessProcessId);

        bool IsCancelled(string businessProcessId);
    }
}