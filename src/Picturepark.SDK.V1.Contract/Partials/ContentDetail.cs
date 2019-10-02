using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract
{
    /// <summary>The content detail.</summary>
    public partial class ContentDetail
    {
        /// <summary>Gets the content detail's file metadata.</summary>
        /// <returns>The file metadata.</returns>
        public FileMetadata GetFileMetadata()
        {
            return Content is FileMetadata metadata ? metadata : ((JObject)Content).ToObject<FileMetadata>();
        }

        /// <summary>Creates a typed content item wrapped in a ContentItem container.</summary>
        /// <typeparam name="T">The content item type.</typeparam>
        /// <returns>The content item.</returns>
        public ContentItem<T> AsContentItem<T>()
        {
            return new ContentItem<T>
            {
                Id = Id,
                ContentPermissionSetIds = ContentPermissionSetIds,
                ContentRights = ContentRights,
                ContentSchemaId = ContentSchemaId,
                ContentType = ContentType,
                DisplayValues = DisplayValues,
                LayerSchemaIds = LayerSchemaIds,
                LifeCycle = LifeCycle,
                Metadata = Metadata,
                Outputs = Outputs,
                Owner = Owner,
                OwnerTokenId = OwnerTokenId,
                Audit = Audit,
                Content = ContentAs<T>()
            };
        }

        /// <summary>
        /// Returns content deserialized into the requested class
        /// </summary>
        /// <typeparam name="T">Content type</typeparam>
        /// <returns>Deserialized content</returns>
        public T ContentAs<T>() => Content is T content ? content : ((JObject)Content).ToObject<T>();

        /// <summary>
        /// Returns layer from <see cref="Metadata"/> deserialized into a custom type.
        /// </summary>
        /// <typeparam name="T">Type representing the layer</typeparam>
        /// <param name="schemaId">Optional ID of the layer within the metadata dictionary. If not provided, it will be determined by schema ID defined in PictureparkSchemaAttribute on the layer class (if applied)
        /// or the name of the type. Anonymous classes are naturally not supported.</param>
        /// <returns>Layer metadata</returns>
        public T Layer<T>(string schemaId = null)
        {
            var layer = Layer(schemaId ?? Contract.Metadata.ResolveLayerKey(typeof(T)));
            return layer != null ? layer.ToObject<T>() : default;
        }

        /// <summary>
        /// Returns layer from <see cref="Metadata"/> based on a schema ID.
        /// </summary>
        /// <param name="schemaId">Schema ID of the layer within the metadata dictionary.</param>
        /// <returns>Layer metadata</returns>
        public JObject Layer(string schemaId)
            => Metadata.TryGetValue(schemaId.ToLowerCamelCase(), out var layer) ? (JObject)layer : null;

        /// <summary>
        /// Tests if a layer is present in <see cref="Metadata"/> dictionary.
        /// </summary>
        /// <typeparam name="T">Type representing the layer</typeparam>
        /// <param name="schemaId">OSchema ID of the layer within the metadata dictionary.</param>
        /// <returns>True if the layer representing the type T is present in metadata dictionary, false otherwise.</returns>
        public bool HasLayer<T>() => HasLayer(Contract.Metadata.ResolveLayerKey(typeof(T)));

        /// <summary>
        /// Tests if a layer is present in <see cref="Metadata"/> dictionary.
        /// </summary>
        /// <param name="schemaId">OSchema ID of the layer within the metadata dictionary.</param>
        /// <returns>True if the layer with the provided schema ID is present in metadata dictionary, false otherwise.</returns>
        public bool HasLayer(string schemaId) => Metadata.ContainsKey(schemaId.ToLowerCamelCase());

        /// <summary>
        /// Deserializes the values for the specified layer and gets the display values.
        /// </summary>
        /// <typeparam name="T">Type representing the layer</typeparam>
        /// <returns>Deserialized display values for the layer</returns>
        public DisplayValueDictionary LayerDisplayValues<T>()
            => LayerDisplayValues(Contract.Metadata.ResolveLayerKey(typeof(T)));

        /// <summary>
        /// Deserializes the values for the specified layer and gets the display values.
        /// </summary>
        /// <param name="schemaId">Layer schema ID</param>
        /// <returns>Deserialized display values for the layer</returns>
        public DisplayValueDictionary LayerDisplayValues(string schemaId)
        {
            var layer = Layer(schemaId);
            return layer?.GetValue("_displayValues")?.ToObject<DisplayValueDictionary>();
        }
    }
}
