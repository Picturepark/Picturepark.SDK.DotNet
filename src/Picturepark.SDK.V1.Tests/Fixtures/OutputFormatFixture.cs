using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class OutputFormatFixture : ClientFixture
    {
        private readonly ConcurrentQueue<string> _createdOutputFormats = new ConcurrentQueue<string>();

        public async Task<IReadOnlyList<OutputFormat>> CreateOutputFormats(int count)
        {
            var createRequests = Enumerable.Range(0, count).Select(_ =>
            {
                var guid = Guid.NewGuid();
                return new OutputFormat
                {
                    Id = $"OF-test-{guid}",
                    Names = new TranslatedStringDictionary
                    {
                        { "en", $"OF_test_{guid}" }
                    },
                    Dynamic = true,
                    Format = new JpegFormat
                    {
                        Quality = 95
                    },
                    SourceOutputFormats = new SourceOutputFormats
                    {
                        Image = "Original",
                        Video = "VideoPreview",
                        Document = "DocumentPreview",
                        Audio = "AudioPreview"
                    }
                };
            }).ToArray();

            var created = await Client.OutputFormat.CreateManyAsync(new OutputFormatCreateManyRequest { Items = createRequests }).ConfigureAwait(false);

            var createdFormats = await Client.OutputFormat.GetManyAsync(created.Rows.Select(r => r.Id)).ConfigureAwait(false);

            foreach (var of in createdFormats)
            {
                _createdOutputFormats.Enqueue(of.Id);
            }

            return createdFormats.ToArray();
        }

        public async Task<OutputFormat> CreateOutputFormat()
        {
            return (await CreateOutputFormats(1)).Single();
        }

        public override void Dispose()
        {
            if (!_createdOutputFormats.IsEmpty)
            {
                foreach (var id in _createdOutputFormats)
                {
                    Client.OutputFormat.DeleteAsync(id).GetAwaiter().GetResult();
                }
            }

            base.Dispose();
        }
    }
}