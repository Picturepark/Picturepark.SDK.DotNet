using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    public partial class OutputFormatClient
    {
        public async Task<ICollection<OutputFormatDetail>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken))
            => await GetManyAsync(cancellationToken: cancellationToken);
    }
}