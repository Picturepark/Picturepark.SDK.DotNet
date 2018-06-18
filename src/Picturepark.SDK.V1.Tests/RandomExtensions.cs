using System;
using System.Collections.Generic;
using System.Linq;

namespace Picturepark.SDK.V1.Tests
{
    public static class RandomExtensions
    {
        public static IEnumerable<T> RandomElements<T>(this ICollection<T> collection, int count)
        {
            var skip = new Random().Next(0, collection.Count - count + 1);
            return collection.Skip(skip).Take(count);
        }
    }
}