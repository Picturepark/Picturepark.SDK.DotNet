using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    public class BatchOperationResultDetail<T>
    {
        internal BatchOperationResultDetail(BusinessProcessDetailsDataBatchResponse details, Func<string[], Task<IEnumerable<T>>> fetchEntities, Func<T, string> idAccessor)
        {
            FailedItems = details.Response.Rows.Where(r => !r.Succeeded).ToArray();
            FailedIds = FailedItems.Select(i => i.Id).ToArray();

            SucceededIds = details.Response.Rows.Where(r => r.Succeeded).Select(i => i.Id).ToArray();
            SucceededItems = new BatchOperationResultCollection<T>(details, fetchEntities);
            SucceededRows = new BatchOperationResultRowCollection<T>(details, fetchEntities, idAccessor);
        }

        /// <summary>
        /// Gets an enumerable containing all successful items of the operation.
        /// </summary>
        /// <remarks>Items are fetched from the server, so make sure to avoid multiple enumerations.</remarks>
        public IEnumerable<T> SucceededItems { get; }

        /// <summary>
        /// Gets an enumerable containing all successful items of the operation and their respective request id.
        /// </summary>
        /// <remarks>Items are fetched from the server, so make sure to avoid multiple enumerations.</remarks>
        public IEnumerable<BatchOperationResultDetailRow<T>> SucceededRows { get; }

        public IReadOnlyList<BatchResponseRow> FailedItems { get; }

        public IReadOnlyList<string> SucceededIds { get; }

        public IReadOnlyList<string> FailedIds { get; }
    }
}