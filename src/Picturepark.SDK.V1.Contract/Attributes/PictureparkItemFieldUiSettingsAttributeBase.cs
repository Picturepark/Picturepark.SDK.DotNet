using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    /// <summary>
    /// Base class to define <see cref="ItemFieldUiSettingsViewItemBase"/> for <see cref="ISchemaClient.GenerateSchemasAsync"/>
    /// </summary>
    public abstract class PictureparkItemFieldUiSettingsAttributeBase : Attribute, IPictureparkAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkItemFieldUiSettingsAttributeBase"/> class.
        /// </summary>
        /// <param name="view">The default view that the UI will use to render the relationship field</param>
        /// <param name="maxListRows">Maximum number of rows for the list view.</param>
        /// <param name="maxThumbRows">Maximum number of rows for the Thumbnail views.</param>
        /// <param name="showRelatedContentOnDownload">Whether related content should be shown in UI download dialog</param>
        protected PictureparkItemFieldUiSettingsAttributeBase(
            ItemFieldViewMode view = ItemFieldViewMode.List,
            int? maxListRows = null,
            int? maxThumbRows = null,
            bool? showRelatedContentOnDownload = null)
        {
            View = view;
            MaxListRows = maxListRows;
            MaxThumbRows = maxThumbRows;
            ShowRelatedContentOnDownload = showRelatedContentOnDownload;
        }

        /// <summary>
        /// Default view that the UI will use to render the field.
        /// </summary>
        public ItemFieldViewMode View { get; }

        /// <summary>
        /// Maximum number of rows for the list view.
        /// </summary>
        public int? MaxListRows { get; }

        /// <summary>
        /// Maximum number of rows for the Thumbnail views.
        /// </summary>
        public int? MaxThumbRows { get; }

        /// <summary>
        /// Whether related content should be shown in UI download dialog
        /// </summary>
        public bool? ShowRelatedContentOnDownload { get; }
    }
}