using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Picturepark.SDK.V1.Contract
{
    public class ReferenceObjectConverter : JsonConverter
    {
        private static readonly ConcurrentDictionary<string, Type> s_cache = new ConcurrentDictionary<string, Type>();

        [ThreadStatic]
        private static bool _tIsReading;

        /// <summary>Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON.</summary>
        public override bool CanWrite => false;

        /// <summary>Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.</summary>
        public override bool CanRead
        {
            get
            {
                if (_tIsReading)
                {
                    _tIsReading = false;
                    return false;
                }

                return true;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);

            var typeName = jObject.ContainsKey("_schemaId") ? jObject["_schemaId"].Value<string>() : Metadata.ResolveSchemaId(objectType);
            var typeToDeserialize = GetObjectSubtype(objectType, typeName);
            return typeToDeserialize != null ? Deserialize(jObject, typeToDeserialize, serializer) : null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.GetTypeInfo().IsSubclassOf(typeof(ReferenceObject));
        }

        private object Deserialize(JObject jObject, Type typeToDeserialize, JsonSerializer serializer)
        {
            try
            {
                _tIsReading = true;
                var deserialized = serializer.Deserialize(jObject.CreateReader(), typeToDeserialize);
                return deserialized;
            }
            finally
            {
                _tIsReading = false;
            }
        }

        private Type GetObjectSubtype(Type objectType, string typeName)
        {
            if (s_cache.TryGetValue(typeName, out var type))
                return type;

            if (typeName == Metadata.ResolveSchemaId(objectType))
            {
                type = objectType;
            }
            else
            {
                var knownTypeAttributes = objectType.GetTypeInfo().GetCustomAttributes().Where(a => a.GetType().Name == "KnownTypeAttribute");

                dynamic knownTypeAttribute = knownTypeAttributes.SingleOrDefault(attribute => IsKnownTypeTargetType(attribute, typeName));
                if (knownTypeAttribute != null)
                    type = knownTypeAttribute.Type;
            }

            s_cache.TryAdd(typeName, type);

            return type;
        }

        private bool IsKnownTypeTargetType(dynamic attribute, string typeName)
        {
            return Metadata.ResolveSchemaId(attribute.Type) == typeName;
        }
    }
}
