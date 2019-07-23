using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    internal class BatchOperationResultRowCollection<T> : IEnumerable<BatchOperationResultDetailRow<T>>
    {
        private readonly Func<string[], Task<IEnumerable<T>>> _fetchEntities;
        private readonly Func<T, string> _idAccessor;
        private readonly BatchResponseRow[] _rows;

        public BatchOperationResultRowCollection(BusinessProcessDetailsDataBatchResponse details, Func<string[], Task<IEnumerable<T>>> fetchEntities, Func<T, string> idAccessor)
        {
            _fetchEntities = fetchEntities;
            _idAccessor = idAccessor;
            _rows = details.Response.Rows.Where(r => r.Succeeded).ToArray();
        }

        public IEnumerator<BatchOperationResultDetailRow<T>> GetEnumerator()
        {
            return new BatchOperationResultRowEnumerator<T>(_rows, _fetchEntities, _idAccessor);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}