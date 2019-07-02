using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IOutputFormatClient
    {
        Task<ICollection<OutputFormatDetail>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}