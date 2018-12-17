using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.Microsite.Example.Contracts;
using Picturepark.SDK.V1.Contract;
using Picturepark.Microsite.Example.Services;
using Picturepark.SDK.V1.Contract.Extensions;

namespace Picturepark.Microsite.Example.Repository
{
    public class PressReleaseRepository : IPressReleaseRepository
    {
        private readonly IPictureparkAccessTokenService _client;

        public PressReleaseRepository(IPictureparkAccessTokenService client)
        {
            _client = client;
        }

        public async Task<List<ContentItem<PressRelease>>> List(int start, int limit, string searchString)
        {
            var searchResult = await _client.Content.SearchAsync(new ContentSearchRequest
            {
                Start = start,
                Limit = limit,
                SearchString = searchString,
                Filter = new AndFilter
                {
                    Filters = new List<FilterBase>
                    {
                        // Limit to PressRelease content
                        FilterBase.FromExpression<Content>(i => i.ContentSchemaId, nameof(PressRelease)),

						// Filter out future publications
						new DateRangeFilter
                        {
                            Field = "pressRelease.publishDate",
                            Range = new DateRange
                            {
                                To = "now"
                            }
                        }
                    }
                },
                Sort = new List<SortInfo>
                {
                    new SortInfo { Field = "pressRelease.publishDate", Direction = SortDirection.Desc }
                }
            });

            // Fetch details
            var contents = searchResult.Results.Any()
                ? await _client.Content.GetManyAsync(searchResult.Results.Select(i => i.Id),
                    new[]
                    {
                        ContentResolveBehavior.Content
                    })
                : new List<ContentDetail>();

            // Convert to C# poco
            var pressPortals = contents.AsContentItems<PressRelease>().ToList();

            return pressPortals;
        }

        public async Task<ContentItem<PressRelease>> Get(string id)
        {
            var content = await _client.Content.GetAsync(id, new[] { ContentResolveBehavior.Content });

            return content.AsContentItem<PressRelease>();
        }

        public async Task<List<SearchResult>> Search(int start, int limit, string searchString)
        {
            var result = await List(start, limit, searchString);

            return result.Select(i => new SearchResult { Id = i.Id, Title = i.Content.Headline.GetTranslation(), Description = i.Content.Teaser.GetTranslation() }).ToList();
        }
    }
}
