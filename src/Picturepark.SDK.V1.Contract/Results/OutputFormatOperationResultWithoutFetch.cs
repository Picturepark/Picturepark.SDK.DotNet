namespace Picturepark.SDK.V1.Contract.Results
{
    public class OutputFormatOperationResultWithoutFetch
    {
        public OutputFormatOperationResultWithoutFetch(string outputFormatId, string businessProcessId, BusinessProcessLifeCycle? lifeCycle)
        {
            OutputFormatId = outputFormatId;
            BusinessProcessId = businessProcessId;
            LifeCycle = lifeCycle;
        }

        public BusinessProcessLifeCycle? LifeCycle { get; }

        public string BusinessProcessId { get; }

        public string OutputFormatId { get; }
    }
}