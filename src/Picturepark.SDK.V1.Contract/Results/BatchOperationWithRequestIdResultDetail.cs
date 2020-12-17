using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public class BatchOperationWithRequestIdResultDetail<T>
    {
        internal BatchOperationWithRequestIdResultDetail(BatchResponseRow[] successfulRows, BatchResponseRow[] failedRows, Func<string[], Task<IEnumerable<T>>> fetchEntities, Func<T, string> idAccessor)
        {
            FailedItems = failedRows;
            FailedIds = FailedItems.Select(i => i.Id).ToArray();

            SucceededIds = successfulRows.Select(i => i.Id).ToArray();
            SucceededItems = new BatchOperationResultRowCollection<T>(successfulRows, fetchEntities, idAccessor);
        }

        /// <summary>
        /// Gets an enumerable containing all successful items of the operation and their respective request id.
        /// </summary>
        /// <remarks>Items are fetched from the server, so make sure to avoid multiple enumerations.</remarks>
        public IEnumerable<BatchOperationResultDetailRow<T>> SucceededItems { get; }

        public IReadOnlyList<BatchResponseRow> FailedItems { get; }

        public IReadOnlyList<string> SucceededIds { get; }

        public IReadOnlyList<string> FailedIds { get; }
    }
}