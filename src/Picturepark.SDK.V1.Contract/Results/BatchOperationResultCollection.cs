using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    internal class BatchOperationResultCollection<T> : IEnumerable<T>
    {
        private readonly Func<string[], Task<IEnumerable<T>>> _fetchEntities;
        private readonly BatchResponseRow[] _rows;

        public BatchOperationResultCollection(BatchResponseRow[] rows, Func<string[], Task<IEnumerable<T>>> fetchEntities)
        {
            _fetchEntities = fetchEntities;
            _rows = rows;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new BatchOperationResultEnumerator<T>(_rows, _fetchEntities);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}