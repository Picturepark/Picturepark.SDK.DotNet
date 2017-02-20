using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests.Contracts
{
	public class JsonInheritanceConverter : JsonConverter
	{
		internal static readonly string DefaultDiscriminatorName = "discriminator";

		[ThreadStatic]
		private static bool t_isReading;

		[ThreadStatic]
		private static bool t_isWriting;

		private static object lockObject = new object();

		private static Dictionary<string, Type> cache = new Dictionary<string, Type>();

		private readonly string _discriminator;

		/// <summary>Initializes a new instance of the <see cref="JsonInheritanceConverter"/> class.</summary>
		public JsonInheritanceConverter()
		{
			_discriminator = DefaultDiscriminatorName;
		}

		/// <summary>Initializes a new instance of the <see cref="JsonInheritanceConverter"/> class.</summary>
		/// <param name="discriminator">The discriminator.</param>
		public JsonInheritanceConverter(string discriminator)
		{
			_discriminator = discriminator;
		}

		/// <summary>Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON.</summary>
		public override bool CanWrite
		{
			get
			{
				if (t_isWriting)
				{
					t_isWriting = false;
					return false;
				}

				return true;
			}
		}

		/// <summary>Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.</summary>
		public override bool CanRead
		{
			get
			{
				if (t_isReading)
				{
					t_isReading = false;
					return false;
				}

				return true;
			}
		}

		/// <summary>Writes the JSON representation of the object.</summary>
		/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			try
			{
				t_isWriting = true;

				var jObject = JObject.FromObject(value, serializer);
				jObject.AddFirst(new JProperty(_discriminator, value.GetType().Name));
				writer.WriteToken(jObject.CreateReader());
			}
			finally
			{
				t_isWriting = false;
			}
		}

		/// <summary>Determines whether this instance can convert the specified object type.</summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns><c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.</returns>
		public override bool CanConvert(Type objectType)
		{
			return true;
		}

		/// <summary>Reads the JSON representation of the object.</summary>
		/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>The object value.</returns>
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var jObject = serializer.Deserialize<JObject>(reader);
			var discriminator = jObject.GetValue(_discriminator).Value<string>();
			var subtype = GetObjectSubtype(objectType, discriminator);

			try
			{
				t_isReading = true;
				return serializer.Deserialize(jObject.CreateReader(), subtype);
			}
			finally
			{
				t_isReading = false;
			}
		}

		private Type GetObjectSubtype(Type objectType, string discriminator)
		{
			if (cache.ContainsKey(discriminator))
			{
				return cache[discriminator];
			}

			Type type = null;

			var knownTypeAttributes = objectType.GetTypeInfo().GetCustomAttributes().Where(a => a.GetType().Name == "KnownTypeAttribute");
			dynamic knownTypeAttribute = knownTypeAttributes.SingleOrDefault(a => IsKnwonTypeTargetType(a, discriminator));
			if (knownTypeAttribute != null)
				type = knownTypeAttribute.Type;

			if (type == null)
			{
				var typeName = objectType.Namespace + "." + discriminator;
				type = objectType.GetTypeInfo().Assembly.GetType(typeName);
			}

			lock (lockObject)
			{
				if (!cache.ContainsKey(discriminator))
					cache.Add(discriminator, type);
			}

			return type;
		}

		private bool IsKnwonTypeTargetType(dynamic attribute, string discriminator)
		{
			return attribute?.Type.Name == discriminator;
		}
	}
}
