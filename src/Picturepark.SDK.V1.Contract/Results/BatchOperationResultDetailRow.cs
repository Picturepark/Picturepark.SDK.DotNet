namespace Picturepark.SDK.V1.Contract.Results
{
    public class BatchOperationResultDetailRow<T>
    {
        internal BatchOperationResultDetailRow(string requestId, T item)
        {
            RequestId = requestId;
            Item = item;
        }

        public T Item { get; }

        public string RequestId { get; }
    }
}