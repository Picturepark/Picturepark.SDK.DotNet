using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract.Results
{
    internal abstract class BatchOperationResultEnumeratorBase<T, TEnumerable> : IEnumerator<TEnumerable>
    {
        private readonly Func<IReadOnlyList<BatchResponseRow>, IEnumerable<T>, IEnumerable<TEnumerable>> _convertor;
        private readonly BatchResponseRow[] _rows;
        private readonly Func<string[], Task<IEnumerable<T>>> _fetchEntities;
        private int _position = -1;
        private TEnumerable[] _currentBatch;
        private int _batchPosition = -1;

        protected BatchOperationResultEnumeratorBase(
            BatchResponseRow[] rows,
            Func<string[], Task<IEnumerable<T>>> fetchEntities,
            Func<IReadOnlyList<BatchResponseRow>, IEnumerable<T>, IEnumerable<TEnumerable>> convertor)
        {
            _convertor = convertor;
            _fetchEntities = fetchEntities;
            _rows = rows;
        }

        public TEnumerable Current { get; private set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _position++;
            _batchPosition++;

            if (_position >= _rows.Length)
                return false;

            if (_currentBatch == null || _batchPosition >= _currentBatch.Length)
            {
                _batchPosition = 0;

                var nextRowBatch = _rows.Skip(_position).Take(100).ToArray();

                _currentBatch = _convertor(
                    nextRowBatch,
                    Task.Run(async () => await _fetchEntities(nextRowBatch.Select(i => i.Id).ToArray()).ConfigureAwait(false)).GetAwaiter()
                        .GetResult()).ToArray();
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