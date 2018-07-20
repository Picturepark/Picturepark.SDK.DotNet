using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    internal class BatchOperationResultCollection<T> : IReadOnlyCollection<T>
    {
        private readonly Func<string[], Task<IEnumerable<T>>> _fetchEntities;
        private readonly string[] _ids;

        public BatchOperationResultCollection(BusinessProcessDetailsDataBatchResponse details, Func<string[], Task<IEnumerable<T>>> fetchEntities)
        {
            _fetchEntities = fetchEntities;
            _ids = details.Response.Rows.Where(r => r.Succeeded).Select(r => r.Id).ToArray();
        }

        public int Count => _ids.Length;

        public IEnumerator<T> GetEnumerator()
        {
            return new BatchOperationResultIterator<T>(_ids, _fetchEntities);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}