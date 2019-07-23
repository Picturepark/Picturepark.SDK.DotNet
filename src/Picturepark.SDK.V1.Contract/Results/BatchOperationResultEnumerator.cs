using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    internal class BatchOperationResultEnumerator<T> : BatchOperationResultEnumeratorBase<T, T>
    {
        public BatchOperationResultEnumerator(
            BatchResponseRow[] rows,
            Func<string[], Task<IEnumerable<T>>> fetchEntities) : base(rows, fetchEntities, (_, items) => items)
        {
        }
    }
}