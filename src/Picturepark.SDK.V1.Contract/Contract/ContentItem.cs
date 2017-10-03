using System;
using System.Collections.Generic;
using System.Text;

namespace Picturepark.SDK.V1.Contract
{
	public class ContentItem<T>
	{
		public string Id { get; set; }

		public T Content { get; set; }

		public StoreAudit Audit { get; set; }
	}
}
