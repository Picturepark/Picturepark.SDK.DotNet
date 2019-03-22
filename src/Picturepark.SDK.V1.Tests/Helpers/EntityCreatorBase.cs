using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Helpers
{
    public abstract class EntityCreatorBase<TEntity> : IDisposable
    {
        protected EntityCreatorBase(IPictureparkService client)
        {
            Client = client;
        }

        protected IPictureparkService Client { get; }

        protected ConcurrentQueue<string> CreatedEntityIds { get; } = new ConcurrentQueue<string>();

        public abstract Task<IReadOnlyList<TEntity>> Create(int count);

        public async Task<TEntity> Create()
        {
            return (await Create(1)).Single();
        }

        public void Dispose()
        {
            if (!CreatedEntityIds.IsEmpty)
            {
                foreach (var id in CreatedEntityIds)
                {
                    try
                    {
                        Delete(id).GetAwaiter().GetResult();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }
        }

        protected abstract Task Delete(string id);

        protected void AddCreated(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                CreatedEntityIds.Enqueue(id);
            }
        }
    }
}