using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract
{
    /// <summary>The content detail.</summary>
    public partial class ContentDetail
    {
        public IReadOnlyList<string> Layers => JMetadata.Properties().Select(p => p.Name).ToList();

        private JObject JMetadata => (JObject)Metadata;

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
            var layer = Layer(name ?? typeof(T).Name);
            return layer.ToObject<T>();
        }

        public JObject Layer(string name)
            => (JObject)JMetadata.GetValue(name.ToLowerCamelCase());

        public bool HasLayer<T>() => HasLayer(typeof(T).Name);

        public bool HasLayer(string name) => JMetadata.ContainsKey(name.ToLowerCamelCase());

        public DisplayValueDictionary LayerDisplayValues<T>()
            => LayerDisplayValues(typeof(T).Name);

        public DisplayValueDictionary LayerDisplayValues(string name)
        {
            var layer = Layer(name);
            return layer.GetValue("_displayValues").ToObject<DisplayValueDictionary>();
        }
    }
}
