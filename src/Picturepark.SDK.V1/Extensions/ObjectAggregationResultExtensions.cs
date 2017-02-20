using System.Linq;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
    public static class ObjectAggregationResultExtensions
    {
        public static AggregationResult GetByName(this ObjectAggregationResult objectAggregationResult, string name)
        {
            return objectAggregationResult.AggregationResults.SingleOrDefault(i => i.Name == name);
        }
    }
}