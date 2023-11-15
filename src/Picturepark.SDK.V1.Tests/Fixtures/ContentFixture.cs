using Picturepark.SDK.V1.Tests.Contracts;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
    public class ContentFixture : ClientFixture
    {
        public ContentFixture()
        {
            Setup().GetAwaiter().GetResult();
        }

        private async Task Setup()
        {
            await SetupSchema<ContentItem>();
            await SetupSchema<AllDataTypesContract>();
            await SetupSchema<PersonShot>();
            await SetupSchema<SimpleLayer>();
        }
    }
}
