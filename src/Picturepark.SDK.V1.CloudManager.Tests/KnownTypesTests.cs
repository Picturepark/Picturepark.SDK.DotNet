using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Picturepark.SDK.V1.CloudManager.Contract;
using Xunit;

namespace Picturepark.SDK.V1.CloudManager.Tests
{
    [Trait("Stack-CloudManager", "KnownTypes")]
    public class KnownTypesTests
    {
        [Fact]
        public void ShouldDeserializeContractsWithKnownTypesAttributeAndMultipleAbstractBaseClasses()
        {
            var customerCreateJson = System.IO.File.ReadAllText(@".\data\customer-create.json");

            var jsonSettings = new JsonSerializerSettings
            {
                Culture = System.Globalization.CultureInfo.InvariantCulture,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            // This request will not fail if the Contracts.Generated.cs is patched temporarily
            // [...] foreach (var type in System.Reflection.CustomAttributeExtensions.GetCustomAttributes<JsonInheritanceAttribute>(System.Reflection.IntrospectionExtensions.GetTypeInfo(objectType), false /* HACK: PP9-5050, true*/))
            // I assume with have to set the inherit flag on GetCustomAttributes(...) to true, to resolve the KnownTypes property.
            var request = JsonConvert.DeserializeObject<CustomerCreateRequest>(customerCreateJson, jsonSettings);

            Assert.NotNull(request);
        }
    }
}
