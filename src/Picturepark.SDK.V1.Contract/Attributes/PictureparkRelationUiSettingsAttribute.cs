using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    /// <summary>
    /// Sets the UI settings of a single / multi relationship field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PictureparkRelationUiSettingsAttribute : PictureparkItemFieldUiSettingsAttributeBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkRelationUiSettingsAttribute"/> class.
        /// </summary>
        /// <param name="view">The default view that the UI will use to render the relationship field</param>
        /// <param name="maxListRows">Maximum number of rows for the list view.</param>
        /// <param name="maxThumbRows">Maximum number of rows for the Thumbnail views.</param>
        /// <param name="showRelatedContentOnDownload">Whether related content should be shown in UI download dialog</param>
        public PictureparkRelationUiSettingsAttribute(
            ItemFieldViewMode view,
            int maxListRows,
            int maxThumbRows,
            bool showRelatedContentOnDownload)
        : base(view, maxListRows, maxThumbRows, showRelatedContentOnDownload)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkRelationUiSettingsAttribute"/> class.
        /// </summary>
        /// <param name="view">The default view that the UI will use to render the field</param>
        /// <param name="showRelatedContentOnDownload">Whether related content should be shown in UI download dialog</param>
        public PictureparkRelationUiSettingsAttribute(
            ItemFieldViewMode view,
            bool showRelatedContentOnDownload)
            : base(view, showRelatedContentOnDownload: showRelatedContentOnDownload)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkRelationUiSettingsAttribute"/> class.
        /// </summary>
        /// <param name="view">The default view that the UI will use to render the field</param>
        /// <param name="maxListRows">Maximum number of rows for the list view.</param>
        /// <param name="maxThumbRows">Maximum number of rows for the Thumbnail views.</param>
        public PictureparkRelationUiSettingsAttribute(
            ItemFieldViewMode view,
            int maxListRows,
            int maxThumbRows)
            : base(view, maxListRows, maxThumbRows)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkRelationUiSettingsAttribute"/> class.
        /// </summary>
        /// <param name="view">The default view that the UI will use to render the relationship field</param>
        public PictureparkRelationUiSettingsAttribute(
            ItemFieldViewMode view)
            : base(view)
        {
        }
    }
}
