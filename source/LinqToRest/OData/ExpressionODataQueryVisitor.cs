using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToRest.OData
{
	public class ExpressionODataQueryVisitor : ODataQueryVisitor
	{
		private bool _isOrdered = false;

		private Type _itemType;

		private Expression _expression;

		public Expression Translate<T>(ODataQuery query)
		{
			var type = typeof (T);

			return Translate(type, query);
		}

		public Expression Translate(Type type, ODataQuery query)
		{
			_itemType = type;

			var parameterType = typeof (IQueryable<>).MakeGenericType(type);

			var parameter = Expression.Parameter(parameterType, "p");

			_expression = parameter;

			Visit(query);

			return Expression.Lambda(_expression, false, parameter);
		}

		protected override ODataFilterQueryPart VisitFilter(ODataFilterQueryPart filter)
		{
			// TODO: build where clause and apply it here
			return base.VisitFilter(filter);
		}

		protected override ODataOrdering VisitOrdering(ODataOrdering ordering)
		{
			string methodName;

			if (_isOrdered == false)
			{
				_isOrdered = true;

				methodName = (ordering.Direction == ODataOrderingDirection.Asc)
					? "OrderBy"
					: "OrderByDescending";
			}
			else
			{
				methodName = (ordering.Direction == ODataOrderingDirection.Asc)
					? "ThenBy"
					: "ThenByDescending";
			}

			var lambda = ExpressionHelper.Lambda(_itemType, ordering.Field);

			_expression = Expression.Call(typeof (Queryable), methodName, new[] {_itemType}, new[] {_expression, lambda});

			return base.VisitOrdering(ordering);
		}

		protected override ODataSkipQueryPart VisitSkip(ODataSkipQueryPart skip)
		{
			if (skip.NumberToSkip.HasValue)
			{
				var skipNumber = Expression.Constant(skip.NumberToSkip.Value, typeof (int));

				_expression = Expression.Call(typeof (Queryable), "Skip", new Type[] {_itemType}, new[] {_expression, skipNumber});
			}

			return base.VisitSkip(skip);
		}

		protected override ODataTopQueryPart VisitTop(ODataTopQueryPart top)
		{
			if (top.NumberToTake.HasValue)
			{
				var takeNumber = Expression.Constant(top.NumberToTake.Value, typeof (int));

				_expression = Expression.Call(typeof (Queryable), "Take", new[] {_itemType}, new[] {_expression, takeNumber});
			}

			return base.VisitTop(top);
		}

		protected override ODataSelectQueryPart VisitSelect(ODataSelectQueryPart select)
		{
			var properties = new List<PropertyInfo>();

			foreach (var selector in select.Selectors)
			{
				if (selector.Instance == null)
				{
					var property = _itemType.GetProperty(selector.Member);

					properties.Add(property);
				}
				else
				{
					throw new ArgumentException(String.Format("Property '{0}' is not on type '{1}'.", selector.Member, _itemType));
				}
			}

			var selectType = AnonymousTypeManager.BuildType(properties);

			var parameter = Expression.Parameter(_itemType);

			var memberBindings = new List<MemberBinding>();

			foreach (var propertyInfo in properties)
			{
				var property = selectType.GetProperty(propertyInfo.Name);

				var memberAccess = Expression.MakeMemberAccess(parameter, property);

				var memberBinding = Expression.Bind(property, memberAccess);

				memberBindings.Add(memberBinding);
			}

			var constructor = Expression.New(selectType);

			var initialization = Expression.MemberInit(constructor, memberBindings);

			var lambda = Expression.Lambda(initialization, false, parameter);

			_expression = Expression.Call(typeof (Queryable), "Select", new[] {_itemType, selectType}, _expression, lambda);

			return base.VisitSelect(select);
		}
	}
}