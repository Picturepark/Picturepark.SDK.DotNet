using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Contract
{
    public partial interface IUserRoleClient
    {
        Task<ICollection<UserRole>> GetAllAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}