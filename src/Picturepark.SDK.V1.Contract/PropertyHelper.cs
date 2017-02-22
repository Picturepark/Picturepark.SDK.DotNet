using System;
using System.Linq.Expressions;
using System.Text;

namespace Picturepark.SDK.V1.Contract
{
	public static class PropertyHelper
	{
		public static string GetName<TObject>(Expression<Func<TObject, object>> propertyRefExpr)
		{
			return GetPropertyPath(propertyRefExpr);
		}

		public static MemberExpression GetMemberExpression(Expression expression)
		{
			if (expression is MemberExpression)
			{
				return (MemberExpression)expression;
			}
			else if (expression is LambdaExpression)
			{
				var lambdaExpression = expression as LambdaExpression;
				if (lambdaExpression.Body is MemberExpression)
				{
					return (MemberExpression)lambdaExpression.Body;
				}
				else if (lambdaExpression.Body is UnaryExpression)
				{
					return (MemberExpression)((UnaryExpression)lambdaExpression.Body).Operand;
				}
			}

			return null;
		}

		public static string GetPropertyPath(Expression expr)
		{
			var path = new StringBuilder();
			MemberExpression memberExpression = GetMemberExpression(expr);
			do
			{
				if (path.Length > 0)
				{
					path.Insert(0, ".");
				}

				path.Insert(0, memberExpression.Member.Name);
				memberExpression = GetMemberExpression(memberExpression.Expression);
			}
			while (memberExpression != null);
			return path.ToString();
		}
	}
}
