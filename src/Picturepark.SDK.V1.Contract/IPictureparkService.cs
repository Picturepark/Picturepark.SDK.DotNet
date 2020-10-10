using System;

namespace Picturepark.SDK.V1.Contract
{
    public interface IPictureparkService : IDisposable
    {
        ISchemaClient Schema { get; }

        IContentClient Content { get; }

        IOutputClient Output { get; }

        IOutputFormatClient OutputFormat { get; }

        IBusinessProcessClient BusinessProcess { get; }

        IDocumentHistoryClient DocumentHistory { get; }

        IInfoClient Info { get; }

        IJsonSchemaClient JsonSchema { get; }

        IListItemClient ListItem { get; }

        ILiveStreamClient LiveStream { get; }

        IContentPermissionSetClient ContentPermissionSet { get; }

        ISchemaPermissionSetClient SchemaPermissionSet { get; }

        ISchemaTransferClient SchemaTransfer { get; }

        IShareClient Share { get; }

        ITransferClient Transfer { get; }

        IUserClient User { get; }

        IUserRoleClient UserRole { get; }

        IProfileClient Profile { get; }

        IChannelClient Channel { get; }

        IBusinessRuleClient BusinessRule { get; }

        IDisplayValueClient DisplayValue { get; }

        IMetadataClient Metadata { get; }

        IIdentityProviderClient IdentityProvider { get; }

        IXmpMappingClient XmpMapping { get; }

        INotificationClient Notification { get; }

        ITemplateClient Template { get; }
    }
}