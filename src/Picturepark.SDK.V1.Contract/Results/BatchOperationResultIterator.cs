using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    internal class BatchOperationResultIterator<T> : IEnumerator<T>
    {
        private readonly string[] _ids;
        private readonly Func<string[], Task<IEnumerable<T>>> _fetchEntities;
        private int _position;
        private T[] _currentBatch;
        private int _batchPosition;

        public BatchOperationResultIterator(string[] ids, Func<string[], Task<IEnumerable<T>>> fetchEntities)
        {
            _ids = ids;
            _fetchEntities = fetchEntities;

            Reset();
        }

        public T Current { get; private set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _position++;
            _batchPosition++;

            if (_position >= _ids.Length)
                return false;

            if (_currentBatch == null || _batchPosition >= _currentBatch.Length)
            {
                _batchPosition = 0;

                var nextIdBatch = _ids.Skip(_position).Take(100).ToArray();
                _currentBatch = Task.Run(async () => await _fetchEntities(nextIdBatch).ConfigureAwait(false)).GetAwaiter().GetResult().ToArray();
            }

            Current = _currentBatch[_batchPosition];
            return true;
        }

        public void Reset()
        {
            _position = -1;
            _batchPosition = -1;
            _currentBatch = null;
        }

        public void Dispose()
        {
        }
    }
}