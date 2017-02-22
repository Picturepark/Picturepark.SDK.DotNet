namespace Picturepark.SDK.V1.Contract
{
    public interface IPictureparkClient
    {
        IMetadataSchemasClient Schemas { get; }

        IAssetsClient Assets { get; }

        IBusinessProcessesClient BusinessProcesses { get; }

        IDocumentHistoryClient DocumentHistory { get; }

        IJsonSchemasClient JsonSchemas { get; }

        IMetadataObjectsClient MetadataObjects { get; }

        IPermissionsClient Permissions { get; }

        IPublicAccessClient PublicAccess { get; }

        ISharesClient Shares { get; }

        ITransfersClient Transfers { get; }

        IUsersClient Users { get; }
    }
}