﻿using System.Linq;

using LinqToRest.Linq;

namespace LinqToRest
{
	public static class WebServices
	{
		public static IQueryable<T> Find<T>()
		{
			return RestQueryableFactory.Create<T>();
		}
	}
}
