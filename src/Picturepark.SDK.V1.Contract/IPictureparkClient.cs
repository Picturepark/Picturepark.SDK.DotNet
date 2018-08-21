using System;

namespace Picturepark.SDK.V1.Contract
{
    public interface IPictureparkClient : IDisposable
    {
        ISchemaClient Schemas { get; }

        IContentClient Contents { get; }

        IOutputClient Outputs { get; }

        IBusinessProcessClient BusinessProcesses { get; }

        IDocumentHistoryClient DocumentHistory { get; }

        IInfoClient Info { get; }

        IJsonSchemaClient JsonSchemas { get; }

        IListItemClient ListItems { get; }

        ILiveStreamClient LiveStream { get; }

        IContentPermissionSetClient ContentPermissionSets { get; }

        ISchemaPermissionSetClient SchemaPermissionSets { get; }

        ISchemaTransferClient SchemaTransfer { get; }

        IPublicAccessClient PublicAccess { get; }

        IShareClient Shares { get; }

        ITransferClient Transfers { get; }

        IUserClient Users { get; }

        IUserRoleClient UserRoles { get; }

        IProfileClient Profile { get; }

        IChannelClient Channels { get; }

        IServiceProviderClient ServiceProviders { get; }

        IShareAccessClient ShareAccess { get; }
    }
}