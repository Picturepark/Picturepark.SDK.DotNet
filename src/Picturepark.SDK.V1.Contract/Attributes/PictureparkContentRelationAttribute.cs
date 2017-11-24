using Picturepark.SDK.V1.Contract.Attributes.Providers;
using System;

namespace Picturepark.SDK.V1.Contract.Attributes
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class PictureparkContentRelationAttribute : Attribute, IPictureparkAttribute
	{
		public PictureparkContentRelationAttribute(string name, string filter)
			: this(name)
		{
			if (!string.IsNullOrEmpty(filter))
			{
				Filter = FilterBase.FromJson(filter);
			}
		}

		public PictureparkContentRelationAttribute(string name, Type filterProvider)
			: this(name)
		{
			if (filterProvider != null)
			{
				var provider = Activator.CreateInstance(filterProvider);
				if (provider is IFilterProvider)
				{
					Filter = ((IFilterProvider)provider).GetFilter();
				}
				else
				{
					throw new ArgumentException("The parameter filterProvider is not of type IFilterProvider.");
				}
			}
		}

		protected PictureparkContentRelationAttribute(string name)
		{
			Name = name;
			TargetContext = TargetContext.Content;
		}

		public TargetContext TargetContext { get; }

		public FilterBase Filter { get; }

		public string Name { get; }
	}
}
