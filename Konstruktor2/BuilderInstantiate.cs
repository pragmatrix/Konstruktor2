using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Konstruktor
{
	partial class Builder
	{
		object instantiate(Type t, IScope scope)
		{
			if (t.IsValueType || Type.GetTypeCode(t) == TypeCode.String)
				throw new ResolveException("failed to instantiate type {0}: no reference or string type", t);

			if (t.IsGenericType)
			{
				var typeDef = t.GetGenericTypeDefinition();
				if (typeDef.Equals(typeof(Func<>)))
					return Func1Factory.instantiate(t, scope);
				if (typeDef.Equals(typeof(Func<,>)))
					return Func2Factory.instantiate(t, scope);
			}

			var instance = instantiateAndWireByReflectionConstructor(t, scope);
			scope.own(instance);
			return instance;
		}

		object instantiateAndWireByReflectionConstructor(Type t, IScope scope)
		{
			var constructors = t.GetConstructors();
			if (constructors.Length == 0)
				throw new ResolveException("failed to instantiate type {0}: no public constructor", t);

			var constructor = selectPreferredConstructor(constructors);
			var parameters = constructor.GetParameters();

			var resolvedObjects = new object[parameters.Length];

			foreach (var p in parameters.indices())
			{
				var obj = scope.resolve(parameters[p].ParameterType);
				resolvedObjects[p] = obj;
			}

			var instance = FormatterServices.GetUninitializedObject(t);
			constructor.Invoke(instance, resolvedObjects);

			scope.Debug("inst {2,8:X}: {0} => {1}".fmt(t.Name, instance, (uint)instance.GetHashCode()));

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
				if (!types[i].Equals(parameters[i].ParameterType))
					return false;

			return true;
		}
	}
}
