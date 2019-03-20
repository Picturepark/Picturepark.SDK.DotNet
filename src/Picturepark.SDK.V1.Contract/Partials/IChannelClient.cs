using System.Collections.Generic;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IChannelClient
    {
        Task<ICollection<Channel>> GetChannelsAsync(System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
    }
}