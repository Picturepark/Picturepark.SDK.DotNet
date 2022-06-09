using System.Collections.Generic;

namespace Picturepark.SDK.V1.Contract.Providers
{
    /// <summary>
    /// Helper to define sort-order using annotation
    /// </summary>
    public interface ISearchSortProvider
    {
        /// <summary>
        /// Returns sort information
        /// </summary>
        /// <returns>List of <see cref="SortInfo"/> in the order it should be applied on SearchRequest</returns>
        IEnumerable<SortInfo> GetSortInfos();
    }
}
