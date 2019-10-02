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

        public T ContentAs<T>() => Content is T content ? content : ((JObject)Content).ToObject<T>();

        public T Layer<T>(string name = null)
        {
            var layer = Layer(name ?? Contract.Metadata.ResolveLayerKey(typeof(T)));
            return layer != null ? layer.ToObject<T>() : default;
        }

        public JObject Layer(string name)
            => Metadata.TryGetValue(name.ToLowerCamelCase(), out var layer) ? (JObject)layer : null;

        public bool HasLayer<T>() => HasLayer(Contract.Metadata.ResolveLayerKey(typeof(T)));

        public bool HasLayer(string name) => Metadata.ContainsKey(name.ToLowerCamelCase());

        public DisplayValueDictionary LayerDisplayValues<T>()
            => LayerDisplayValues(Contract.Metadata.ResolveLayerKey(typeof(T)));

        public DisplayValueDictionary LayerDisplayValues(string name)
        {
            var layer = Layer(name);
            return layer?.GetValue("_displayValues")?.ToObject<DisplayValueDictionary>();
        }
    }
}
