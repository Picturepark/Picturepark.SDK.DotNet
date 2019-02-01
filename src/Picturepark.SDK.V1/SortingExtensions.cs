using System;
using System.Collections.Generic;
using System.Linq;

namespace Picturepark.SDK.V1
{
    internal static class SortingExtensions
    {
        public static IEnumerable<T> TopoSort<T>(this IEnumerable<T> items, Func<T, IEnumerable<T>> dependencyAccessor)
        {
            // create a stack that contains a ValueTuple for each element in items, the boolean flag indicates if the item has already been sorted
            var stack = new Stack<ValueTuple<T, bool>>(items.Select(s => (s, false)));
            var alreadyReturned = new HashSet<T>();
            var alreadyVisited = new HashSet<T>();

            while (stack.Count > 0)
            {
                var (item, sorted) = stack.Pop();

                if (sorted)
                {
                    // we have a sorted value but did already return it, something is wrong.
                    if (!alreadyReturned.Add(item))
                    {
                        throw new Exception("Unexpected error");
                    }

                    yield return item;
                }
                else
                {
                    if (alreadyVisited.Add(item))
                    {
                        // mark this node as sorted and add all the dependencies, marking them as not yet sorted.
                        stack.Push((item, true));

                        foreach (var dependency in dependencyAccessor(item))
                        {
                            stack.Push((dependency, false));
                        }
                    }
                    else if (!alreadyReturned.Contains(item))
                    {
                        // abort because of cyclic dependency, we already visited this node, but we did not yet return it.
                        throw new Exception("Cyclic dependency detected");
                    }
                }
            }
        }
    }
}