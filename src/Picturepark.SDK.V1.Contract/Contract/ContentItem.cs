namespace Picturepark.SDK.V1.Contract
{
    public class ContentItem<T>
    {
        /// <summary>The content id.</summary>
        public string Id { get; set; }

        /// <summary>The content data</summary>
        public T Content { get; set; }

        /// <summary>Audit data with information regarding document creation and modification.</summary>
        public UserAuditDetail Audit { get; set; }

        /// <summary>An optional id list of content permission sets. Controls content accessibility outside of content ownership.</summary>
        public System.Collections.Generic.ICollection<string> ContentPermissionSetIds { get; set; }

        /// <summary>The id of the content schema</summary>
        public string ContentSchemaId { get; set; }

        /// <summary>The type of content</summary>
        public ContentType ContentType { get; set; }

        /// <summary>Contains language specific display values, rendered according to the content schema's
        ///              display pattern configuration.</summary>
        public DisplayValueDictionary DisplayValues { get; set; }

        /// <summary>An optional list of layer schemas ids</summary>
        public System.Collections.Generic.ICollection<string> LayerSchemaIds { get; set; } = new System.Collections.Generic.List<string>();

        /// <summary>The metadata dictionary</summary>
        public DataDictionary Metadata { get; set; }

        /// <summary>A list of rendering outputs for underlying digital file.</summary>
        public System.Collections.Generic.ICollection<Output> Outputs { get; set; }

        /// <summary>The owner token ID. Defines the content owner.</summary>
        public string OwnerTokenId { get; set; }

        /// <summary>The resolved owner.</summary>
        public User Owner { get; set; }

        /// <summary>The lifecycle of the content.</summary>
        public LifeCycle LifeCycle { get; set; }

        /// <summary>List of content rights the user has on this content</summary>
        public System.Collections.Generic.ICollection<ContentRight> ContentRights { get; set; }
    }
}
