using System.Collections.Generic;

namespace Picturepark.SDK.V1.Contract
{
	public partial class SchemaDetail
	{
		public ICollection<SchemaDetail> Dependencies { get; } = new List<SchemaDetail>();
	}
}
