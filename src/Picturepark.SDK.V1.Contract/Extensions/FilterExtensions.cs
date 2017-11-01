using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Picturepark.SDK.V1.Contract.Extensions
{
	public static class FilterExtensions
	{
		public static FilterBase TermFilter<TObject>(this FilterBase filter, Expression<Func<TObject, object>> propertyRefExpr, string term)
		{
			var name = PropertyHelper.GetName(propertyRefExpr);
			return new TermFilter { Field = name, Term = term };
		}

		public static FilterBase TermsFilter<TObject>(this FilterBase filter, Expression<Func<TObject, object>> propertyRefExpr, List<string> terms)
		{
			var name = PropertyHelper.GetName(propertyRefExpr);
			return new TermsFilter { Field = name, Terms = terms };
		}
	}
}
