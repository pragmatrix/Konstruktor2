using System;
using System.Diagnostics;
using System.Reflection;
using Konstruktor.Detail;

namespace Konstruktor
{
	partial class Konstruktor
	{
		public object build(Type t, IKonstruktorScope konstruktorScope)
		{
#if DEBUG
			Debug.Assert(_frozen);
#endif
			Func<IKonstruktorScope, object> explicitGenerator;
			if (_explicitGenerators.TryGetValue(t, out explicitGenerator))
				return buildByExplicitGenerator(explicitGenerator, konstruktorScope);

			return t.IsInterface ? buildFromInterface(t, konstruktorScope) : instantiate(t, konstruktorScope);
		}

		static object buildByExplicitGenerator(Func<IKonstruktorScope, object> explicitGenerator, IKonstruktorScope konstruktorScope)
		{
			var instance = explicitGenerator(konstruktorScope);
			konstruktorScope.own(instance);
			return instance;
		}

		object buildFromInterface(Type t, IKonstruktorScope konstruktorScope)
		{
			Debug.Assert(t.IsInterface);
			var implementation = tryGetImplementationForInterface(t);
			if (implementation != null)
				return konstruktorScope.resolve(implementation);

			if (t.IsGenericType)
				return buildFromGenericInterface(t, konstruktorScope);

			throw new ResolveException("failed to resolve type {0}: failed to resolve interface", t);
		}

		object buildFromGenericInterface(Type genericInterface, IKonstruktorScope konstruktorScope)
		{
			Debug.Assert(genericInterface.IsInterface && genericInterface.IsGenericType && !genericInterface.IsGenericTypeDefinition);
			var def = genericInterface.GetGenericTypeDefinition();
			var implementation = tryGetImplementationForInterface(def);
			if (implementation == null)
				return buildFromGenericByUsingAGenerator(genericInterface, konstruktorScope);

			Debug.Assert(implementation.GetGenericArguments().Length == genericInterface.GetGenericArguments().Length);

			var implementationType = implementation.MakeGenericType(genericInterface.GetGenericArguments());

			return konstruktorScope.resolve(implementationType);
		}

		Type tryGetImplementationForInterface(Type interfaceType)
		{
			Type rType;
			_interfaceToImplementation.TryGetValue(interfaceType, out rType);

			return rType;
		}

		object buildFromGenericByUsingAGenerator(Type genericInterface, IKonstruktorScope konstruktorScope)
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

			var generated_ = callStaticMethod(concreteMethod, konstruktorScope);
			// a generated object must be owned by the scope!
			konstruktorScope.own(generated_);

			konstruktorScope.Debug("gent {2,8:X}: {0} => {1}".fmt(genericInterface.Name, generated_, generated_ == null ? 0 : (uint)generated_.GetHashCode()));

			return generated_;
		}

		MethodInfo tryResolveFactoryMethod(Type interfaceTypeDef)
		{
			Debug.Assert(interfaceTypeDef.IsGenericTypeDefinition);
			MethodInfo method;
			_generatorMethods.TryGetValue(interfaceTypeDef, out method);
			return method;
		}

		static object callStaticMethod(MethodInfo concreteMethod, IKonstruktorScope konstruktorScope)
		{
			Debug.Assert(!concreteMethod.IsGenericMethodDefinition);
			Debug.Assert(concreteMethod.IsStatic);

			var parameters = concreteMethod.GetParameters();
			var arguments = new object[parameters.Length];
			foreach (var p in parameters.indices())
				arguments[p] = konstruktorScope.resolve(parameters[p].ParameterType);

			return concreteMethod.Invoke(null, arguments);
		}
	}
}
