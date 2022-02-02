using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    /// <summary>
    /// Sets the UI settings of a single / multi relationship field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PictureparkRelationUiSettingsAttribute : Attribute, IPictureparkAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkRelationUiSettingsAttribute"/> class.
        /// </summary>
        /// <param name="view">Default view that the UI will use to render a single / multi relationship field.</param>
        public PictureparkRelationUiSettingsAttribute(RelationView view)
        {
            View = view;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkRelationUiSettingsAttribute"/> class.
        /// </summary>
        /// <param name="view">The default view that the UI will use to render the relationship field</param>
        /// <param name="maxListRows">Maximum number of rows for the list view.</param>
        /// <param name="maxThumbRows">Maximum number of rows for the Thumbnail views.</param>
        public PictureparkRelationUiSettingsAttribute(RelationView view, int maxListRows, int maxThumbRows) : this(view)
        {
            MaxListRows = maxListRows;
            MaxThumbRows = maxThumbRows;
        }

        /// <summary>
        /// Default view that the UI will use to render a single / multi relationship field.
        /// </summary>
        public RelationView View { get; }

        /// <summary>
        /// Maximum number of rows for the list view.
        /// </summary>
        public int? MaxListRows { get; }

        /// <summary>
        /// Maximum number of rows for the Thumbnail views.
        /// </summary>
        public int? MaxThumbRows { get; }
    }
}
