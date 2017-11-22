using System;
using System.Linq;
using System.Linq.Expressions;

namespace Picturepark.SDK.V1.Contract
{
	public partial class FilterBase
	{
		public static TermFilter FromExpression<TObject>(Expression<Func<TObject, object>> propertyRefExpr, string term)
		{
			var name = PropertyHelper.GetLowerCamelCasePropertyPath(propertyRefExpr);
			return new TermFilter { Field = name, Term = term };
		}

		public static TermsFilter FromExpression<TObject>(Expression<Func<TObject, object>> propertyRefExpr, params string[] terms)
		{
			var name = PropertyHelper.GetLowerCamelCasePropertyPath(propertyRefExpr);
			return new TermsFilter { Field = name, Terms = terms.ToList() };
		}
	}
}
