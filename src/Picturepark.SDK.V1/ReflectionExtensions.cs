using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Picturepark.SDK.V1
{
    internal static class ReflectionExtensions
    {
        public static IEnumerable<Type> GetKnownTypes(this Type type)
        {
            foreach (var knownTypeAttribute in type.GetTypeInfo().GetCustomAttributes<KnownTypeAttribute>())
            {
                if (knownTypeAttribute.Type != null)
                {
                    yield return knownTypeAttribute.Type;
                }
                else if (knownTypeAttribute.MethodName != null)
                {
                    var method = type.GetRuntimeMethod(knownTypeAttribute.MethodName, new Type[0]);
                    if (method != null)
                    {
                        var knownTypes = (IEnumerable<Type>)method.Invoke(null, new object[0]);
                        foreach (var knownType in knownTypes)
                        {
                            yield return knownType;
                        }
                    }
                }
            }
        }
    }
}