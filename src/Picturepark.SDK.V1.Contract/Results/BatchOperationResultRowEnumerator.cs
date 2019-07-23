using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    internal class BatchOperationResultRowEnumerator<T> : BatchOperationResultEnumeratorBase<T, BatchOperationResultDetailRow<T>>
    {
        public BatchOperationResultRowEnumerator(
            BatchResponseRow[] rows,
            Func<string[], Task<IEnumerable<T>>> fetchEntities,
            Func<T, string> idAccessor)
            : base(
                rows,
                fetchEntities,
                (rowSubset, items) =>
                {
                    var requestIds = rows.ToDictionary(x => x.Id, x => x.RequestId);
                    return items.Select(i => new BatchOperationResultDetailRow<T>(requestIds[idAccessor(i)], i));
                })
        {
        }
    }
}