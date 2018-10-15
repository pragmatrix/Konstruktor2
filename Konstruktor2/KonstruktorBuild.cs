using System;
using System.Diagnostics;
using System.Reflection;
using Konstruktor2.Detail;

namespace Konstruktor2
{
	partial class Konstruktor
	{
		/*
			This method is the only entry point to build and instantiate an instance of type t that is 
			bound to the given lifetime scope.
		*/

		object IKonstruktor.build(ILifetimeScope lifetimeScope, Type t)
		{
#if DEBUG
			lifetimeScope.Debug("building {0}".fmt(t.Name));

			Debug.Assert(_frozen);
#endif
			if (_explicitGenerators.TryGetValue(t, out var explicitGenerator))
				return buildByExplicitGenerator(explicitGenerator, lifetimeScope);

			var ti = t.GetTypeInfo();
			return ti.IsInterface ? buildFromInterface(lifetimeScope, t) : instantiate(lifetimeScope, t);
		}

		static object buildByExplicitGenerator(Func<ILifetimeScope, object> explicitGenerator, ILifetimeScope lifetimeScope)
		{
			var instance = explicitGenerator(lifetimeScope);
			lifetimeScope.own(instance);
			return instance;
		}

		object buildFromInterface(ILifetimeScope lifetimeScope, Type t)
		{
			var ti = t.GetTypeInfo();
			Debug.Assert(ti.IsInterface);
			var implementation = tryGetImplementationForInterface(t);
			if (implementation != null)
				return lifetimeScope.resolve(implementation);

			if (ti.IsGenericType)
				return buildFromGenericInterface(lifetimeScope, t);

			throw new ResolveException("failed to resolve type {0}: failed to resolve interface", t);
		}

		object buildFromGenericInterface(ILifetimeScope lifetimeScope, Type genericInterface)
		{
			var genericInterfaceTI = genericInterface.GetTypeInfo();
			Debug.Assert(genericInterfaceTI.IsInterface && genericInterfaceTI.IsGenericType && !genericInterfaceTI.IsGenericTypeDefinition);
			var def = genericInterface.GetGenericTypeDefinition();
			var implementation = tryGetImplementationForInterface(def);
			if (implementation == null)
				return buildFromGenericByUsingAGenerator(lifetimeScope, genericInterface);

			var implementationTI = implementation.GetTypeInfo();
			Debug.Assert(implementationTI.GetGenericArguments().Length == genericInterfaceTI.GetGenericArguments().Length);

			var implementationType = implementation.MakeGenericType(genericInterfaceTI.GetGenericArguments());

			return lifetimeScope.resolve(implementationType);
		}

		Type tryGetImplementationForInterface(Type interfaceType)
		{
			_interfaceToImplementation.TryGetValue(interfaceType, out var rType);

			return rType;
		}

		object buildFromGenericByUsingAGenerator(ILifetimeScope lifetimeScope, Type genericInterface)
		{
			var genericInterfaceTypeInfo = genericInterface.GetTypeInfo();
			Debug.Assert(genericInterfaceTypeInfo.IsInterface && genericInterfaceTypeInfo.IsGenericType && !genericInterfaceTypeInfo.IsGenericTypeDefinition);
			var def = genericInterface.GetGenericTypeDefinition();

			var factoryMethod = tryResolveFactoryMethod(def);

			if (factoryMethod == null)
				throw new ResolveException("failed to resolve generic type {0}: failed to resolve interface", genericInterface);

			var arguments = genericInterfaceTypeInfo.GetGenericArguments();
			var methodTypeArguments = factoryMethod.GetGenericArguments();
			if (arguments.Length != methodTypeArguments.Length)
				throw new ResolveException("failed to find generators method for type {0}, number of type arguments are not matching", genericInterface);

			var concreteMethod = factoryMethod.MakeGenericMethod(arguments);

			var generated_ = callStaticMethod(lifetimeScope, concreteMethod);
			// a generated object must be owned by the scope!
			lifetimeScope.own(generated_);

			lifetimeScope.Debug("gent {2,8:X}: {0} => {1}".fmt(genericInterface.Name, generated_, generated_ == null ? 0 : (uint)generated_.GetHashCode()));

			return generated_;
		}

		MethodInfo tryResolveFactoryMethod(Type interfaceTypeDef)
		{
			var interfaceTypeDefInfo = interfaceTypeDef.GetTypeInfo();
			Debug.Assert(interfaceTypeDefInfo.IsGenericTypeDefinition);
			_generatorMethods.TryGetValue(interfaceTypeDef, out var method);
			return method;
		}

		static object callStaticMethod(ILifetimeScope lifetimeScope, MethodInfo concreteMethod)
		{
			Debug.Assert(!concreteMethod.IsGenericMethodDefinition);
			Debug.Assert(concreteMethod.IsStatic);

			var parameters = concreteMethod.GetParameters();
			var arguments = new object[parameters.Length];
			foreach (var p in parameters.indices())
				arguments[p] = lifetimeScope.resolve(parameters[p].ParameterType);

			return concreteMethod.Invoke(null, arguments);
		}
	}
}
