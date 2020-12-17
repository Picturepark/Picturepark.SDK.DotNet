using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public class BatchOperationResultDetail<T>
    {
        internal BatchOperationResultDetail(BatchResponseRow[] successfulRows, IReadOnlyList<BatchResponseRow> failedRows, Func<string[], Task<IEnumerable<T>>> fetchEntities)
        {
            FailedItems = failedRows;
            FailedIds = FailedItems.Select(i => i.Id).ToArray();

            SucceededIds = successfulRows.Select(i => i.Id).ToArray();
            SucceededItems = new BatchOperationResultCollection<T>(successfulRows, fetchEntities);
        }

        /// <summary>
        /// Gets an enumerable containing all successful items of the operation.
        /// </summary>
        /// <remarks>Items are fetched from the server, so make sure to avoid multiple enumerations.</remarks>
        public IEnumerable<T> SucceededItems { get; }

        public IReadOnlyList<BatchResponseRow> FailedItems { get; }

        public IReadOnlyList<string> SucceededIds { get; }

        public IReadOnlyList<string> FailedIds { get; }
    }
}