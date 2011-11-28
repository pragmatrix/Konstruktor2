using System;
using System.Diagnostics;
using System.Reflection;

namespace Konstruktor
{
	partial class Builder
	{
		public object build(Type t, ILifetimeScope lifetimeScope)
		{
#if DEBUG
			Debug.Assert(_frozen);
#endif
			Func<ILifetimeScope, object> explicitGenerator;
			if (_explicitGenerators.TryGetValue(t, out explicitGenerator))
				return buildByExplicitGenerator(explicitGenerator, lifetimeScope);

			return t.IsInterface ? buildFromInterface(t, lifetimeScope) : instantiate(t, lifetimeScope);
		}

		static object buildByExplicitGenerator(Func<ILifetimeScope, object> explicitGenerator, ILifetimeScope lifetimeScope)
		{
			var instance = explicitGenerator(lifetimeScope);
			lifetimeScope.own(instance);
			return instance;
		}

		object buildFromInterface(Type t, ILifetimeScope lifetimeScope)
		{
			Debug.Assert(t.IsInterface);
			var implementation = tryGetImplementationForInterface(t);
			if (implementation != null)
				return lifetimeScope.resolve(implementation);

			if (t.IsGenericType)
				return buildFromGenericInterface(t, lifetimeScope);

			throw new ResolveException("failed to resolve type {0}: failed to resolve interface", t);
		}

		object buildFromGenericInterface(Type genericInterface, ILifetimeScope lifetimeScope)
		{
			Debug.Assert(genericInterface.IsInterface && genericInterface.IsGenericType && !genericInterface.IsGenericTypeDefinition);
			var def = genericInterface.GetGenericTypeDefinition();
			var implementation = tryGetImplementationForInterface(def);
			if (implementation == null)
				return buildFromGenericByUsingAGenerator(genericInterface, lifetimeScope);

			Debug.Assert(implementation.GetGenericArguments().Length == genericInterface.GetGenericArguments().Length);

			var implementationType = implementation.MakeGenericType(genericInterface.GetGenericArguments());

			return lifetimeScope.resolve(implementationType);
		}

		Type tryGetImplementationForInterface(Type interfaceType)
		{
			Type rType;
			_interfaceToImplementation.TryGetValue(interfaceType, out rType);

			return rType;
		}

		object buildFromGenericByUsingAGenerator(Type genericInterface, ILifetimeScope lifetimeScope)
		{
			Debug.Assert(genericInterface.IsInterface && genericInterface.IsGenericType && !genericInterface.IsGenericTypeDefinition);
			var def = genericInterface.GetGenericTypeDefinition();

			var factoryMethod = tryResolveFactoryMethod(def);

			if (factoryMethod == null)
				throw new ResolveException("failed to resolve generic type {0}: failed to resolve interface", genericInterface);

			var arguments = genericInterface.GetGenericArguments();
			var methodTypeArguments = factoryMethod.GetGenericArguments();
			if (arguments.Length != methodTypeArguments.Length)
				throw new ResolveException("failed to find generators method for type {0}, number of type arguments are not matching", genericInterface);

			var concreteMethod = factoryMethod.MakeGenericMethod(arguments);

			var generated_ = callStaticMethod(concreteMethod, lifetimeScope);
			// a generated object must be owned by the scope!
			lifetimeScope.own(generated_);

			lifetimeScope.Debug("gent {2,8:X}: {0} => {1}".fmt(genericInterface.Name, generated_, generated_ == null ? 0 : (uint)generated_.GetHashCode()));

			return generated_;
		}

		MethodInfo tryResolveFactoryMethod(Type interfaceTypeDef)
		{
			Debug.Assert(interfaceTypeDef.IsGenericTypeDefinition);
			MethodInfo method;
			_generatorMethods.TryGetValue(interfaceTypeDef, out method);
			return method;
		}

		static object callStaticMethod(MethodInfo concreteMethod, ILifetimeScope lifetimeScope)
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
