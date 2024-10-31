﻿using Newtonsoft.Json;
using System;
using Picturepark.SDK.V1.Contract.Providers;

namespace Picturepark.SDK.V1.Contract.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class PictureparkTagboxAttribute : Attribute, IPictureparkAttribute
    {
        public PictureparkTagboxAttribute(string filter = null)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                Filter = JsonConvert.DeserializeObject<FilterBase>(filter);
            }
        }

        public PictureparkTagboxAttribute(Type filterProvider)
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

        public FilterBase Filter { get; }
    }
}
