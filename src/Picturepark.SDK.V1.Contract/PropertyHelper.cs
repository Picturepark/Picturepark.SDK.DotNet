using System;
using System.Linq.Expressions;
using System.Text;

namespace Picturepark.SDK.V1.Contract
{
    public static class PropertyHelper
    {
        public static string ToLowerCamelCase(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        public static string GetLowerCamelCasePropertyPath<TObject>(Expression<Func<TObject, object>> propertyRefExpr)
        {
            return GetLowerCamelCasePropertyPath((Expression)propertyRefExpr);
        }

        public static string GetPropertyName(Expression expr)
        {
            return GetMemberExpression(expr).Member.Name;
        }

        private static string GetLowerCamelCasePropertyPath(Expression expr)
        {
            var path = new StringBuilder();
            var memberExpression = GetMemberExpression(expr);
            do
            {
                if (path.Length > 0)
                {
                    path.Insert(0, ".");
                }

                path.Insert(0, memberExpression.Member.Name.ToLowerCamelCase());
                memberExpression = GetMemberExpression(memberExpression.Expression);
            }
            while (memberExpression != null);

            return path.ToString();
        }

        private static MemberExpression GetMemberExpression(Expression expression)
        {
            switch (expression)
            {
                case MemberExpression memberExpression:
                    return memberExpression;
                case LambdaExpression lambdaExpression:
                    switch (lambdaExpression.Body)
                    {
                        case MemberExpression memberExpression2:
                            return memberExpression2;
                        case UnaryExpression unaryExpression:
                            return (MemberExpression)unaryExpression.Operand;
                    }

                    break;
            }

            return null;
        }
    }
}
