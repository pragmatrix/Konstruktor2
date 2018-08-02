using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Konstruktor2.Detail;

namespace Konstruktor2
{
	partial class Konstruktor
	{
		object instantiate(ILifetimeScope lifetimeScope, Type t)
		{
			var ti = t.GetTypeInfo();
			if (ti.IsValueType || Type.GetTypeCode(t) == TypeCode.String)
				throw new ResolveException("failed to instantiate type {0}: no reference or string type", t);

			if (ti.IsGenericType)
			{
				var typeDef = t.GetGenericTypeDefinition();
				if (typeDef == typeof(Func<>))
					return Func1Factory.instantiate(t, lifetimeScope);
				if (typeDef == typeof(Func<,>))
					return Func2Factory.instantiate(t, lifetimeScope);
			}

			var instance = instantiateByReflectionConstructor(lifetimeScope, t);
			lifetimeScope.own(instance);
			return instance;
		}

		object instantiateByReflectionConstructor(ILifetimeScope lifetimeScope, Type t)
		{
			var ti = t.GetTypeInfo();
			var constructors = ti.GetConstructors();
			if (constructors.Length == 0)
				throw new ResolveException("failed to instantiate type {0}: no public constructor", t);

			var constructor = selectPreferredConstructor(constructors);
			var parameters = constructor.GetParameters();

			var resolvedObjects = new object[parameters.Length];

			foreach (var p in parameters.indices())
			{
				var obj = lifetimeScope.resolve(parameters[p].ParameterType);
				resolvedObjects[p] = obj;
			}

			var instance = constructor.Invoke(resolvedObjects);

			lifetimeScope.Debug("inst {2,8:X}: {0} => {1}".fmt(t.Name, instance, (uint)instance.GetHashCode()));

			return instance;
		}

		ConstructorInfo selectPreferredConstructor(ConstructorInfo[] constructors)
		{
			Debug.Assert(constructors.Length >= 1);
			if (constructors.Length == 1)
				return constructors[0];

			return (from constructor in constructors
					let parameters = constructor.GetParameters()
					orderby
						(isPreferredConstructor(constructor) ? 1 : 0) descending,
						constructor.GetParameters().Length descending
					select constructor).First();
		}

		bool isPreferredConstructor(ConstructorInfo constructorInfo)
		{
			Type[] preferred;
			if (_preferredConstructor.TryGetValue(constructorInfo.DeclaringType, out preferred)
				&& compareTypes(preferred, constructorInfo.GetParameters()))
				return true;

			if (constructorInfo.hasAttribute<PreferredConstructorAttribute>())
				return true;

			return false;
		}

		static bool compareTypes(Type[] types, ParameterInfo[] parameters)
		{
			if (types.Length != parameters.Length)
				return false;

			for (int i = 0; i != types.Length; ++i)
				if (types[i] != parameters[i].ParameterType)
					return false;

			return true;
		}
	}
}
