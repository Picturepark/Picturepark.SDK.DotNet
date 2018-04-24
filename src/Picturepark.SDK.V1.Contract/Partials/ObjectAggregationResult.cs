using System.Linq;

namespace Picturepark.SDK.V1.Contract
{
    public partial class ObjectAggregationResult
    {
        /// <summary>Gets an aggregation result by name.</summary>
        /// <param name="name">The name.</param>
        /// <returns>The aggregation result or null.</returns>
        public AggregationResult GetByName(string name)
        {
            // TODO: Rename to TryGetByName because it may return null?
            return AggregationResults.SingleOrDefault(i => i.Name == name);
        }
    }
}
