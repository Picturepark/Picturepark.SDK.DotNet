using Picturepark.SDK.V1.Tests.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests.Fixtures
{
	public class BusinessProcessFixture : SDKClientFixture
	{
		public BusinessProcessFixture()
		{
			Setup().Wait();
		}

		public async Task Setup()
		{
			if (await Client.Schemas.ExistsAsync(nameof(BusinessProcessTest)) == false)
			{
				var schema = Client.Schemas.GenerateSchemaFromPOCO(typeof(BusinessProcessTest));
				await Client.Schemas.CreateAsync(schema.First(), true);
			}
		}
	}
}
