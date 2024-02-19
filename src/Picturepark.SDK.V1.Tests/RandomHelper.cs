using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests
{
    public class RandomHelper
    {
        private static readonly Random Random = new Random();
        private static readonly object RandomLock = new object();

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
                int randomNumber = SafeRandomNext(result.Results.Count);
                contentId = result.Results.Skip(randomNumber).First().Id;
            }

            return contentId;
        }

        private static int SafeRandomNext(int max)
        {
            lock (RandomLock)
            {
                return Random.Next(0, max);
            }
        }
    }
}
