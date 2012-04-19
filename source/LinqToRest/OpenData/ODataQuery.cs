using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToRest.OpenData
{
	public abstract class ODataQuery
	{
		public virtual string ExpandPredicate { get; set; }
		public virtual string FilterPredicate { get; set; }
		public virtual ODataFormat Format { get; set; }
		public virtual string OrderByPredicate { get; set; }
		public virtual string SelectPredicate { get; set; }
		public virtual int? Skip { get; set; }
		public virtual string SkipToken { get; set; }
		public virtual int? Top { get; set; }
	}

	public class ODataQuery<T> : ODataQuery
	{
		public virtual void SetFilter(Expression<Func<T, bool>> constraint)
		{
			var filterExpressionVisitor = new ODataExpressionVisitor();

			var predicate = filterExpressionVisitor.Translate(constraint);

			FilterPredicate = predicate;
		}

		public virtual void SetSelect<TProperty>(Expression<Func<T, TProperty>> propertyAccessor)
		{
			var body = propertyAccessor.Body as MemberExpression;

			if (body == null)
			{
				var ctor = propertyAccessor.Body as NewExpression;

				foreach (var argument in ctor.Arguments)
				{
					var member = argument as MemberExpression;
				}
			}
			else
			{
				SelectPredicate = body.Member.Name;
			}
		}

		public virtual void SetSkipToken<TProperty>(Expression<Func<T, TProperty>> propertyAccessor, TProperty tokenValue)
		{
			var body = propertyAccessor.Body as MemberExpression;

			if (body == null)
			{
				throw new ArgumentException("Could not convert to MemberExpression.");
			}

			OrderByPredicate = body.Member.Name;
			SkipToken = String.Format("{0}", tokenValue);
		}

		public virtual void SetOrderBy<TProperty>(Expression<Func<T, TProperty>> propertyAccessor)
		{
			var body = propertyAccessor.Body as MemberExpression;

			if (body == null)
			{
				throw new ArgumentException("Could not convert to MemberExpression.");
			}

			OrderByPredicate = body.Member.Name;
		}

		public override string ToString()
		{
			var oDataQueryParts = new List<string>(8);

			if (String.IsNullOrWhiteSpace(ExpandPredicate) == false)
			{
				oDataQueryParts.Add(String.Format("$expand={0}", ExpandPredicate));
			}

			if (String.IsNullOrWhiteSpace(FilterPredicate) == false)
			{
				oDataQueryParts.Add(String.Format("$filter={0}", FilterPredicate));
			}

			oDataQueryParts.Add(String.Format("$format={0}", Format.ToString().ToLowerInvariant()));

			if (String.IsNullOrWhiteSpace(OrderByPredicate) == false)
			{
				oDataQueryParts.Add(String.Format("$orderby={0}", OrderByPredicate));
			}

			if (String.IsNullOrWhiteSpace(SelectPredicate) == false)
			{
				oDataQueryParts.Add(String.Format("$select={0}", SelectPredicate));
			}

			if (Skip.HasValue)
			{
				oDataQueryParts.Add(String.Format("$skip={0}", Skip));
			}

			if (String.IsNullOrWhiteSpace(SkipToken) == false)
			{
				oDataQueryParts.Add(String.Format("$skiptoken={0}", SkipToken));
			}

			if (Top.HasValue)
			{
				oDataQueryParts.Add(String.Format("$top={0}", Top));
			}

			return String.Join("&", oDataQueryParts);
		}
	}
}