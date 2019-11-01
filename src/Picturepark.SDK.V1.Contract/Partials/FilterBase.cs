using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Extensions;

namespace Picturepark.SDK.V1.Contract
{
    public partial class FilterBase
    {
        private static readonly IDictionary<Analyzer, string> AnalyzerToSuffixMappingOverride = new Dictionary<Analyzer, string>();

        /// <summary>Creates a new <see cref="TermFilter"/> whose property name is retrieved from an expression.</summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <param name="property">The property expression.</param>
        /// /// <param name="analyzer">Specify if you want to query the analyzed field version</param>
        /// <param name="term">The search term.</param>
        /// <returns>The term filter.</returns>
        public static TermFilter FromExpression<TObject>(Expression<Func<TObject, object>> property, string term, Analyzer analyzer = Analyzer.None)
            => new TermFilter { Field = BuildFieldPath(property, analyzer), Term = term };

        /// <summary>Creates a new <see cref="TermFilter"/> whose property name is retrieved from an expression.</summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <param name="property">The property expression.</param>
        /// <param name="term">The search term.</param>
        /// <param name="language">Specify the language to query inside the TranslatedStringDictionary</param>
        /// <param name="useAnalyzer">Specify whether to use the language analyzed version</param>
        /// <returns>The term filter.</returns>
        public static TermFilter FromExpression<TObject>(
            Expression<Func<TObject, TranslatedStringDictionary>> property, string term, string language = null, bool useAnalyzer = false)
        {
            var name = BuildFieldPath(property);
            var nameMaybeLang = (name, language).JoinByDot();
            var fieldPath = (nameMaybeLang, AnalyzerToSuffix(useAnalyzer ? Analyzer.Language : Analyzer.None)).JoinByDot();
            return new TermFilter { Field = fieldPath, Term = term };
        }

        /// <summary>Creates a new <see cref="TermsFilter"/> whose property name is retrieved from an expression.</summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <param name="property">The property expression.</param>
        /// <param name="terms">The search terms.</param>
        /// <returns>The terms filter.</returns>
        public static TermsFilter FromExpression<TObject>(Expression<Func<TObject, object>> property, params string[] terms)
            => new TermsFilter { Field = BuildFieldPath(property), Terms = terms };

        /// <summary>Creates a new <see cref="TermsFilter"/> whose property name is retrieved from an expression.</summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <param name="property">The property expression.</param>
        /// <param name="analyzer">Specify if you want to query the analyzed field version</param>
        /// <param name="terms">The search terms.</param>
        /// <returns>The terms filter.</returns>
        public static TermsFilter FromExpression<TObject>(Expression<Func<TObject, object>> property, Analyzer analyzer, params string[] terms)
            => new TermsFilter { Field = BuildFieldPath(property, analyzer), Terms = terms };

        /// <summary>Creates a new <see cref="TermsFilter"/> whose property name is retrieved from an expression.</summary>
        /// <typeparam name="TObject">The object type.</typeparam>
        /// <param name="property">The property expression.</param>
        /// <param name="terms">The search terms.</param>
        /// <param name="language">Specify the language to query inside the TranslatedStringDictionary</param>
        /// <returns>The terms filter.</returns>
        public static TermsFilter FromExpression<TObject>(
            Expression<Func<TObject, TranslatedStringDictionary>> property, string language, IEnumerable<string> terms)
        {
            var name = BuildFieldPath(property);
            var fieldPath = (name, language).JoinByDot();
            return new TermsFilter { Field = fieldPath, Terms = terms.ToArray() };
        }

        private static string AnalyzerToSuffix(Analyzer analyzer)
            => AnalyzerToSuffixMappingOverride.TryGetValue(analyzer, out var suffix)
                ? suffix
                : (analyzer != Analyzer.None ? analyzer.ToString().ToLower() : null);

        private static string BuildFieldPath<TObject, TResult>(Expression<Func<TObject, TResult>> propertyExpression, Analyzer analyzer = Analyzer.None)
        {
            var schemaAttr = typeof(TObject).GetTypeInfo().GetCustomAttribute<PictureparkSchemaAttribute>();
            var systemSchemaAttr = typeof(TObject).GetTypeInfo().GetCustomAttribute<PictureparkSystemSchemaAttribute>();

            var maybeSchemaName =
                schemaAttr != null || systemSchemaAttr != null ? (schemaAttr?.Id ?? typeof(TObject).Name).ToLowerCamelCase() : null;

            var nameAndMaybeSchema = (maybeSchemaName, PropertyHelper.GetLowerCamelCasePropertyPath(propertyExpression)).JoinByDot();

            return (nameAndMaybeSchema, AnalyzerToSuffix(analyzer)).JoinByDot();
        }
    }
}
