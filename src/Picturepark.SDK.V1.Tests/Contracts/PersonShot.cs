using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Attributes.Analyzer;
using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Picturepark.SDK.V1.Tests.Contracts
{
	[PictureparkSchemaType(SchemaType.Layer)]
	public class PersonShot
	{
		public List<Person> Persons { get; set; }

		public List<Country> Countries { get; set; }

		[PictureparkSimpleAnalyzer(SimpleSearch = true)]
		public string Description { get; set; }

		[PictureparkPathHierarchyAnalyzer(SimpleSearch = true)]
		public string Path { get; set; }
	}
}
