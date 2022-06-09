using System;
using System.Collections.Generic;
using System.Linq;
using Picturepark.SDK.V1.Contract.Providers;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    /// <summary>
    /// Defines sorting to be used with <see cref="FieldDynamicView.FilterTemplate"/>. Can be applied more than once.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class PictureparkDynamicViewSortAttribute : Attribute, IPictureparkAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkDynamicViewSortAttribute"/> class.
        /// </summary>
        /// <param name="fieldPath">Path in form {layerId}.{fieldId}</param>
        /// <param name="direction">Direction for sorting on this field</param>
        public PictureparkDynamicViewSortAttribute(string fieldPath, SortDirection direction)
        {
            SortInfos = new[] { new SortInfo { Field = fieldPath, Direction = direction } };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PictureparkDynamicViewSortAttribute"/> class.
        /// </summary>
        /// <param name="sortProviderType">Type implementing <see cref="ISearchSortProvider"/> to provide sorting information</param>
        /// <exception cref="ArgumentException">Passed type is not <see cref="ISearchSortProvider"/></exception>
        public PictureparkDynamicViewSortAttribute(Type sortProviderType)
        {
            if (Activator.CreateInstance(sortProviderType) is ISearchSortProvider provider)
                SortInfos = provider.GetSortInfos().ToList();
            else
                throw new ArgumentException($"The parameter {nameof(sortProviderType)} is not of type {nameof(ISearchSortProvider)}.");
        }

        /// <summary>
        /// Ordered sort information to be applied in search request
        /// </summary>
        public IReadOnlyList<SortInfo> SortInfos { get; }
    }
}
