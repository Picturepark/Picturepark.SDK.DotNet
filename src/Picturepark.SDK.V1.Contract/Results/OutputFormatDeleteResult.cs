namespace Picturepark.SDK.V1.Contract.Results
{
    public class OutputFormatDeleteResult : OutputFormatOperationResultWithoutFetch
    {
        public OutputFormatDeleteResult(string outputFormatId, string businessProcessId, BusinessProcessLifeCycle? lifeCycle) : base(outputFormatId, businessProcessId, lifeCycle)
        {
        }
    }
}