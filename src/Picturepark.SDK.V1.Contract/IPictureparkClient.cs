namespace Picturepark.SDK.V1.Contract
{
    public interface IPictureparkClient
    {
        ISchemaClient Schemas { get; }

        IContentClient Contents { get; }

        IBusinessProcessClient BusinessProcesses { get; }

        IDocumentHistoryClient DocumentHistory { get; }

        IJsonSchemaClient JsonSchemas { get; }

        IListItemClient ListItems { get; }

        IPermissionClient Permissions { get; }

        IPublicAccessClient PublicAccess { get; }

        IShareClient Shares { get; }

        ITransferClient Transfers { get; }

        IUserClient Users { get; }
    }
}