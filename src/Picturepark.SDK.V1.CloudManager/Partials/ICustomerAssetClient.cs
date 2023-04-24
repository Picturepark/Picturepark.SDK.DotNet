using System.Threading.Tasks;

namespace Picturepark.SDK.V1.CloudManager.Contract;

/// <summary>
/// Customer asset client endpoints
/// </summary>
public partial interface ICustomerAssetClient
{
    Task PutLogoAsync(
        string customerId,
        LogoKind type,
        FileParameter file,
        System.Threading.CancellationToken cancellationToken = default);

    Task PutWatermarkAsync(
        string customerId,
        FileParameter file,
        System.Threading.CancellationToken cancellationToken = default);
}