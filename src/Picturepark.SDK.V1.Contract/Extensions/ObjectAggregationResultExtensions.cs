using System.Linq;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class ObjectAggregationResultExtensions
    {
        public static AggregationResult GetByName(this ObjectAggregationResult objectAggregationResult, string name)
        {
            return objectAggregationResult.AggregationResults.SingleOrDefault(i => i.Name == name);
        }
    }
}