using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests
{
    public class RandomHelper
    {
        public static async Task<ContentSearchResult> GetRandomContentsAsync(IPictureparkService client, string searchString, int limit, IReadOnlyList<ContentType> contentTypes = null)
        {
            var request = new ContentSearchRequest { SearchString = searchString, Limit = limit };
            if (contentTypes?.Any() == true)
            {
                request.Filter = new TermsFilter
                {
                    Field = nameof(ContentDetail.ContentType).ToLowerCamelCase(),
                    Terms = contentTypes.Select(ct => ct.ToString()).ToArray()
                };
            }

            return await client.Content.SearchAsync(request);
        }

        public static async Task<string> GetRandomContentIdAsync(IPictureparkService client, string searchString, int limit)
        {
            string contentId = string.Empty;
            ContentSearchRequest request = new ContentSearchRequest { Limit = limit };

            if (!string.IsNullOrEmpty(searchString))
                request.SearchString = searchString;

            ContentSearchResult result = await client.Content.SearchAsync(request);

            if (result.Results.Count > 0)
            {
                int randomNumber = new Random().Next(0, result.Results.Count);
                contentId = result.Results.Skip(randomNumber).First().Id;
            }

            return contentId;
        }

        public static async Task<string> GetRandomContentPermissionSetIdAsync(IPictureparkService client, int limit)
        {
            string permissionSetId = string.Empty;
            PermissionSetSearchRequest request = new PermissionSetSearchRequest { Limit = limit };
            PermissionSetSearchResult result = await client.ContentPermissionSet.SearchAsync(request).ConfigureAwait(false);

            if (result.Results.Count > 0)
            {
                var randomNumber = new Random().Next(0, result.Results.Count);
                permissionSetId = result.Results.Skip(randomNumber).First().Id;
            }

            return permissionSetId;
        }

        public static async Task<string> GetRandomFileTransferIdAsync(IPictureparkService client, int limit)
        {
            string fileTransferId = string.Empty;
            FileTransferSearchRequest request = new FileTransferSearchRequest() { Limit = limit };
            FileTransferSearchResult result = await client.Transfer.SearchFilesAsync(request);

            if (result.Results.Count > 0)
            {
                int randomNumber = new Random().Next(0, result.Results.Count);
                fileTransferId = result.Results.Skip(randomNumber).First().Id;
            }

            return fileTransferId;
        }

        public static async Task<string> GetRandomSchemaPermissionSetIdAsync(IPictureparkService client, int limit)
        {
            string permissionSetId = string.Empty;
            var request = new PermissionSetSearchRequest { Limit = limit };
            PermissionSetSearchResult result = await client.SchemaPermissionSet.SearchAsync(request).ConfigureAwait(false);

            if (result.Results.Count > 0)
            {
                int randomNumber = new Random().Next(0, result.Results.Count);
                permissionSetId = result.Results.Skip(randomNumber).First().Id;
            }

            return permissionSetId;
        }

        public static async Task<string> GetRandomShareIdAsync(IPictureparkService client, ShareType shareType, int limit)
        {
            var shareId = string.Empty;

            var request = new ShareSearchRequest
            {
                Limit = limit,
                Filter = new TermFilter { Field = "shareType", Term = shareType.ToString() }
            };

            var result = await client.Share.SearchAsync(request);

            var shares = result.Results;
            if (shares.Count > 0)
            {
                var randomNumber = new Random().Next(0, shares.Count);
                shareId = shares.Skip(randomNumber).First().Id;
            }

            return shareId;
        }
    }
}
