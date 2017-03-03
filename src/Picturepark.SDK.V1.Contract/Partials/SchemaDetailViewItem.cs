using System.Collections.Generic;

namespace Picturepark.SDK.V1.Contract
{
	public partial class SchemaDetailViewItem
	{
		public ICollection<SchemaDetailViewItem> Dependencies { get; } = new List<SchemaDetailViewItem>();
	}
}
