using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract
{
	public partial class DataDictionary
	{
		/// <summary>Gets a specific item based on the <paramref name="schemaId"/> from the dictionary and deserializes the object to the given type.</summary>
		/// <typeparam name="T">Type to deserialize.</typeparam>
		/// <param name="schemaId">The schema ID for the dictionary lookup.</param>
		/// <returns>The deserialized object.d</returns>
		public T Get<T>(string schemaId)
		{
			return this[schemaId] is T ? (T)this[schemaId] : ((JObject)this[schemaId]).ToObject<T>();
		}
	}
}
