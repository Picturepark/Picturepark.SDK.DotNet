using Newtonsoft.Json.Linq;
using System;

namespace Picturepark.SDK.V1.Contract
{
    public partial class ListItem
    {
        /// <summary>Converts the content of a list item detail to the given type.</summary>
        /// <typeparam name="T">The content type.</typeparam>
        /// <param name="schemaId">The schema ID.</param>
        /// <returns>The converted content.</returns>
        public T ConvertTo<T>(string schemaId)
        {
            // TODO: ListItem.ConvertTo: Why is schemaId needed here? / is this check really necessary?
            if (ContentSchemaId != schemaId)
            {
                throw new InvalidOperationException("The schema IDs do not match.");
            }

            return Content is T ? (T)Content : ((JObject)Content).ToObject<T>();
        }
    }
}
