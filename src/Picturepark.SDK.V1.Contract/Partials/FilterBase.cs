using System;
using System.Linq;
using System.Linq.Expressions;

namespace Picturepark.SDK.V1.Contract
{
    public partial class FilterBase
    {
        /// <summary>Creates a new <see cref="TermFilter"/> whose property name is retrieved from an expression.</summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="term">The search term.</param>
        /// <returns>The term filter.</returns>
        public static TermFilter FromExpression<TObject>(Expression<Func<TObject, object>> propertyExpression, string term)
        {
            var name = PropertyHelper.GetLowerCamelCasePropertyPath(propertyExpression);
            return new TermFilter { Field = name, Term = term };
        }

        /// <summary>Creates a new <see cref="TermsFilter"/> whose property name is retrieved from an expression.</summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <param name="propertyExpression">The property expression.</param>
        /// <param name="terms">The search terms.</param>
        /// <returns>The terms filter.</returns>
        public static TermsFilter FromExpression<TObject>(Expression<Func<TObject, object>> propertyExpression, params string[] terms)
        {
            var name = PropertyHelper.GetLowerCamelCasePropertyPath(propertyExpression);
            return new TermsFilter { Field = name, Terms = terms };
        }
    }
}
