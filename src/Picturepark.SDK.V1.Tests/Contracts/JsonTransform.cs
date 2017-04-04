using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Picturepark.SDK.V1.Tests.Contracts
{
	[PictureparkSchemaType(SchemaType.Struct)]
	public class JsonTransform
	{
		[JsonIgnore]
		public string IgnoredString { get; set; }

		[JsonProperty("_newName")]
		public string OldName { get; set; }

		[PictureparkContentRelation(
			"RelationName",
			"{ 'Kind': 'TermFilter', 'Field': 'ContentType', Term: 'Bitmap' }"
		)]
		public SimpleRelation RelationField { get; set; }
	}
}
