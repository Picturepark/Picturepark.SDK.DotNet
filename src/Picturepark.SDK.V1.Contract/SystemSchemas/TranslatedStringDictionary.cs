using Picturepark.SDK.V1.Contract.Attributes;
using System.Collections.Generic;

namespace Picturepark.SDK.V1.Contract
{
	[PictureparkSystemSchema]
	public partial class TranslatedStringDictionary
	{
		public TranslatedStringDictionary()
		{
		}

		public TranslatedStringDictionary(int capacity)
			: base(capacity)
		{
		}

		public TranslatedStringDictionary(IDictionary<string, string> dictionary)
		{
			if (dictionary == null)
				return;

			foreach (var item in dictionary)
				Add(item.Key, item.Value);
		}
	}
}
