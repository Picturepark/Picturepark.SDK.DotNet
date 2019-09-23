using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Picturepark.SDK.V1.Contract
{
    public static class Metadata
    {
        public static IDictionary<string, object> From(params object[] layers)
        {
            return layers.ToDictionary(l => l.GetType().Name.ToLowerCamelCase());
        }

        public static IDictionary<string, object> From(string layerName, object layer)
        {
            return new Dictionary<string, object> { [layerName.ToLowerCamelCase()] = layer };
        }

        public static IDictionary<string, object> Update<T, TProperty>(Expression<Func<T, TProperty>> property, TProperty newValue)
        {
            var name = ((PropertyInfo)((MemberExpression)property.Body).Member).Name;

            return new Dictionary<string, object> { [typeof(T).Name.ToLowerCamelCase()] = new Dictionary<string, object> { [name] = newValue } };
        }
    }
}