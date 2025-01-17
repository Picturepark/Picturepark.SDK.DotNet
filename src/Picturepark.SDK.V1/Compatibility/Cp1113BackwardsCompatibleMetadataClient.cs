using System;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Compatibility;

internal sealed class Cp1113BackwardsCompatibleMetadataClient : IMetadataClient
{
    private readonly IMetadataClient _metadataClientImplementation;

    public Cp1113BackwardsCompatibleMetadataClient(IMetadataClient metadataClientImplementation)
    {
        _metadataClientImplementation = metadataClientImplementation;
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public async Task<MetadataStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var status = await _metadataClientImplementation.GetStatusAsync(cancellationToken);

        if (status.ListSchemaIds is not null && status.ContentOrLayerSchemaIds is not null)
        {
            status.MainDocuments ??= new MetadataStatusEntries
            {
                ListSchemaIds = status.ListSchemaIds,
                ContentOrLayerSchemaIds = status.ContentOrLayerSchemaIds
            };
        }

        status.SearchDocuments ??= new MetadataStatusEntries
        {
            ListSchemaIds = Array.Empty<string>(),
            ContentOrLayerSchemaIds = Array.Empty<string>()
        };

        return status;
    }
#pragma warning restore CS0618 // Type or member is obsolete

    public async Task<BusinessProcess> UpdateOutdatedAsync(CancellationToken cancellationToken = default)
    {
        return await _metadataClientImplementation.UpdateOutdatedAsync(cancellationToken);
    }
}
