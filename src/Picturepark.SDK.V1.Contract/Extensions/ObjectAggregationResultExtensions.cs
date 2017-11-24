using System.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
	public static class ObjectAggregationResultExtensions
	{
		/// <summary>Gets an aggregation result by name.</summary>
		/// <param name="objectAggregationResult">The object aggregation result.</param>
		/// <param name="name">The name.</param>
		/// <returns>The aggregation result or null.</returns>
		public static AggregationResult GetByName(this ObjectAggregationResult objectAggregationResult, string name)
		{
			// TODO: Rename to TryGetByName because it may return null?
			return objectAggregationResult.AggregationResults.SingleOrDefault(i => i.Name == name);
		}
	}
}