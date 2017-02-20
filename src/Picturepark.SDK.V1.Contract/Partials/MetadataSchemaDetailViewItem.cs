using System.Collections.Generic;

namespace Picturepark.SDK.V1.Contract
{
	public partial class MetadataSchemaDetailViewItem
	{
		public ICollection<MetadataSchemaDetailViewItem> Dependencies { get; } = new List<MetadataSchemaDetailViewItem>();
	}
}
