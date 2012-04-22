﻿using System;
using System.Collections.Generic;
using System.Linq;

using StructureMap;

namespace LinqToRest.StructureMap
{
	public class StructureMapDependencyResolver : IDependencyResolver
	{
		private readonly IContainer _container;

		public StructureMapDependencyResolver() : this(ObjectFactory.Container)
		{
		}

		public StructureMapDependencyResolver(IContainer container)
		{
			_container = container;
		}

		public object GetInstance(Type type)
		{
			object result;

			if (type.IsAbstract || type.IsInterface)
			{
				result = _container.TryGetInstance(type);
			}
			else
			{
				result = _container.GetInstance(type);
			}

			return result;
		}

		public IEnumerable<object> GetAllInstances(Type type)
		{
			return _container.GetAllInstances(type).Cast<object>();
		}
	}
}