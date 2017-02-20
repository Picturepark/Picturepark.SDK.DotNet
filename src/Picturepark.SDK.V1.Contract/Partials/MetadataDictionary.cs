using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract
{
    public partial class MetadataDictionary
    {
        public T Get<T>()
        {
            var schemaId = nameof(T);
            return Get<T>(schemaId);
        }

        public T Get<T>(string schemaId)
        {
            return ((JObject)this[schemaId]).ToObject<T>();
        }
    }
}
