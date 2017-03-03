using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract
{
	public partial class MetadataDictionary
	{
		/// <summary>
		/// Gets a specific item (schemaId) from the dictionary and deserializes the object to a c# poco
		/// </summary>
		/// <typeparam name="T">Type to deserialize</typeparam>
		/// <param name="schemaId">SchemaId for dictionary lookup</param>
		/// <returns>Deserialized object</returns>
		public T Get<T>(string schemaId)
		{
			return ((JObject)this[schemaId]).ToObject<T>();
		}
	}
}
