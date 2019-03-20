using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    public partial class ChannelClient
    {
        [Obsolete("Please use GetAllAsync method instead.")]
        public async Task<ICollection<Channel>> GetChannelsAsync(System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
            => await GetAllAsync(cancellationToken).ConfigureAwait(false);
    }
}