using System;
using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract.Providers;
using Picturepark.SDK.V1.Contract.SystemTypes;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    /// <summary>
    /// Used to specify options for a field of type <see cref="FieldDynamicView"/>
    /// </summary>
    /// <remarks>Use with property of type <see cref="DynamicViewObject"/></remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PictureparkDynamicViewAttribute : Attribute, IPictureparkAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkDynamicViewAttribute"/> class.
        /// </summary>
        /// <param name="filter">JSON-serialized filter</param>
        /// <exception cref="ArgumentNullException"><paramref name="filter"/> is null or whitespace</exception>
        public PictureparkDynamicViewAttribute(string filter) : this()
        {
            if (string.IsNullOrEmpty(filter))
                throw new ArgumentNullException(nameof(filter));

            FilterTemplate = JsonConvert.DeserializeObject<FilterBase>(filter);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkDynamicViewAttribute"/> class.
        /// </summary>
        /// <param name="filterProvider"><see cref="IFilterProvider"/> to obtain instance of <see cref="FilterBase"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="filterProvider"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="filterProvider"/> is of wrong type</exception>
        public PictureparkDynamicViewAttribute(Type filterProvider) : this()
        {
            if (Activator.CreateInstance(filterProvider) is IFilterProvider provider)
                FilterTemplate = provider.GetFilter();
            else
                throw new ArgumentException($"The parameter {nameof(filterProvider)} is not of type {nameof(IFilterProvider)}.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkDynamicViewAttribute"/> class.
        /// </summary>
        protected PictureparkDynamicViewAttribute()
        {
            TargetDocType = "Content";
        }

        /// <summary>
        /// Please refer to <see cref="FieldDynamicView.TargetDocType"/>
        /// </summary>
        public string TargetDocType { get; }

        /// <summary>
        /// Please refer to <see cref="FieldDynamicView.FilterTemplate"/>
        /// </summary>
        public FilterBase FilterTemplate { get; }
    }
}
