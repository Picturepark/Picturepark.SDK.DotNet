using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Contract
{
    public static class Metadata
    {
        /// <summary>
        /// Creates a metadata dictionary from a layer or a set of layers.
        /// </summary>
        /// <param name="layers">Objects containing data for respective layers</param>
        /// <returns>Metadata dictionary that can be used in a Metadata property of various contracts, e.g. <see cref="ContentCreateRequest"/></returns>
        /// <remarks>Layer names (schema IDs) will be determined by schema ID defined in PictureparkSchemaAttribute on the layer class (if applied) or the name of the type.
        /// For use with anonymous classes, please use the <see cref="From(string, object)"/> overload.</remarks>
        public static IDictionary<string, object> From(params object[] layers)
        {
            return layers.ToDictionary(l => ResolveLayerKey(l.GetType()));
        }

        /// <summary>
        /// Creates a metadata dictionary from a layer. Usually used if the layer object is an anonymous object or a dictionary.
        /// </summary>
        /// <param name="layerName">Schema ID of the layer schema</param>
        /// <param name="layer">Object containing data of the layer</param>
        /// <returns>Metadata dictionary that can be used in a Metadata property of various contracts, e.g. <see cref="ContentCreateRequest"/></returns>
        public static IDictionary<string, object> From(string layerName, object layer)
        {
            return new Dictionary<string, object> { [layerName.ToLowerCamelCase()] = layer };
        }

        /// <summary>
        /// Creates a metadata dictionary for a use in a metadata update scenario.
        /// </summary>
        /// <param name="property">The property on the object (schema field) you want to update</param>
        /// <param name="newValue">New value you want to update the schema field to</param>
        /// <returns>Metadata dictionary that can be used in a Metadata property of update contracts, e.g. <see cref="ContentMetadataUpdateRequest"/></returns>
        /// <remarks>Layer name (schema ID) will be determined by schema ID defined in PictureparkSchemaAttribute on the layer class (if applied) or the name of the type.
        /// Anonymous classes are naturally not supported.</remarks>
        public static IDictionary<string, object> Update<T, TProperty>(Expression<Func<T, TProperty>> property, TProperty newValue)
        {
            var name = ((PropertyInfo)((MemberExpression)property.Body).Member).Name;

            return new Dictionary<string, object> { [ResolveLayerKey(typeof(T))] = new Dictionary<string, object> { [name] = newValue } };
        }

        /// <summary>
        /// Resolves schema ID based on either the ID defined in <see cref="PictureparkSchemaAttribute"/> (if applied on the type) or based on type name.
        /// </summary>
        /// <param name="type">Type for which to resolve schema ID</param>
        /// <returns>Resolved schema ID</returns>
        public static string ResolveSchemaId(Type type) => type.GetTypeInfo().GetCustomAttribute<PictureparkSchemaAttribute>()?.Id ?? type.Name;

        /// <summary>
        /// Resolves schema ID based on either the ID defined in <see cref="PictureparkSchemaAttribute"/> (if applied on the type) or based on type name.
        /// </summary>
        /// <param name="obj">Object for which to resolve schema ID</param>
        /// <returns>Resolved schema ID</returns>
        public static string ResolveSchemaId(object obj) => ResolveSchemaId(obj.GetType());

        /// <summary>
        /// Resolves layer key for metadata dictionary based on either the ID defined in <see cref="PictureparkSchemaAttribute"/> (if applied on the type) or based on type name.
        /// </summary>
        /// <param name="layerType">Type for which to resolve layer key</param>
        /// <returns>Resolved layer key</returns>
        public static string ResolveLayerKey(Type layerType) => ResolveSchemaId(layerType).ToLowerCamelCase();
    }
}
