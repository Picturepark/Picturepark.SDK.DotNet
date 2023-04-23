using Picturepark.SDK.V1.CloudManager.Contract;

namespace Picturepark.SDK.V1.CloudManager;

public partial interface ICustomerAssetClient
{
    System.Threading.Tasks.Task PutLogoAsync(
        string customerId,
        LogoKind type,
        string path,
        System.Threading.CancellationToken cancellationToken = default);
}