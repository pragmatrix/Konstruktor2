﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Konstruktor
{
	interface IBuilder
	{
		object build(Type t, IScope scope);
	}

	public sealed partial class Builder : IBuilder
	{
		readonly Dictionary<Type, Func<IScope, object>> _explicitGenerators = new Dictionary<Type, Func<IScope, object>>();
		readonly Dictionary<Type, Type> _interfaceToImplementation = new Dictionary<Type, Type>();
		readonly Dictionary<Type, MethodInfo> _generatorMethods = new Dictionary<Type, MethodInfo>();
		readonly Dictionary<Type, Type[]> _preferredConstructor = new Dictionary<Type, Type[]>();

#if DEBUG
		bool _frozen;
#endif

		public void generator<GeneratedT>(Func<IScope, GeneratedT> constructor)
		{
			_explicitGenerators.Add(typeof(GeneratedT), scope => constructor(scope));
		}

		[Obsolete("Use forInterface<>()")]
		public void iface<InterfaceT, ImplementationT>()
		{
			var iType = typeof (InterfaceT);
			var cType = typeof (ImplementationT);
			Debug.Assert(iType.IsAssignableFrom(cType));
			iface(iType, cType);
		}

		public ForInterface<InterfaceT> forInterface<InterfaceT>()
			where InterfaceT:class
		{
			return new ForInterface<InterfaceT>(this);
		}

		public sealed class ForInterface<InterfaceT>
			where InterfaceT:class
		{
			readonly Builder _builder;

			public ForInterface(Builder builder)
			{
				_builder = builder;
			}

			public void use<ImplementationT>()
				where ImplementationT : InterfaceT
			{
				_builder.iface(typeof (InterfaceT), typeof (ImplementationT));
			}
		}

		public void iface(Type interfaceType, Type implementationType)
		{
#if DEBUG
			Debug.Assert(!_frozen);
#endif
			if (!interfaceType.IsInterface)
				throw new ArgumentException("must be an interface", "interfaceType");

			if (implementationType.IsInterface)
				throw new ArgumentException("must be an implementation type", "implementationType");

			if (interfaceType.IsGenericTypeDefinition != implementationType.IsGenericTypeDefinition)
				throw new ArgumentException("none or both must be open generic types", "interfaceType, implementationType");

			if (interfaceType.IsGenericTypeDefinition && interfaceType.GetGenericArguments().Length != implementationType.GetGenericArguments().Length)
				throw new ArgumentException("number of generic arguments do not match", "interfaceType, implementationType");

			if (!interfaceType.IsGenericTypeDefinition && !interfaceType.IsAssignableFrom(implementationType))
				throw new ArgumentException("interface is not implemented by the implementation", "interfaceType, implementationType");

			if (_interfaceToImplementation.ContainsKey(interfaceType))
				this.Debug("Reregistering implementation type {0} for interface {1}".fmt(implementationType.Name, interfaceType.Name));

			_interfaceToImplementation[interfaceType] = implementationType;
		}

		public void generators<FactoryT>()
		{
			generators(typeof(FactoryT));
		}

		public void generators(Type type)
		{
#if DEBUG
			Debug.Assert(!_frozen);
#endif
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			var factoryMethods = from m in methods where m.hasAttribute<GeneratorMethodAttribute>() select m;

			foreach (var fm in factoryMethods)
			{
				var returnType = fm.ReturnType;
				var typeDefinition = returnType.IsGenericType ? returnType.GetGenericTypeDefinition() : returnType;
				_generatorMethods.Add(typeDefinition, fm);
			}
		}

		public void preferConstructor<TypeT>(params Type[] constructorTypes)
		{
			_preferredConstructor[typeof (TypeT)] = constructorTypes;
		}

		public void scanAssembly(Assembly assembly)
		{
			var defaultAttr = typeof (DefaultImplementationAttribute);

			foreach (var implementationType in assembly.GetTypes())
			{
				var attrs = (DefaultImplementationAttribute[]) implementationType.GetCustomAttributes(defaultAttr, false);
				foreach (var attr in attrs)
				{
					var interfaceTypes = attr.InterfaceTypes;
					if (interfaceTypes.Length == 0)
					{
						registerImplementation(implementationType);
						continue;
					}

					foreach (var interfaceType in interfaceTypes)
					{
						Debug.Assert(interfaceType.IsAssignableFrom(implementationType));
						iface(interfaceType, implementationType);
					}
				}
			}
		}

		void registerImplementation(Type implementationType)
		{
			foreach (var interfaceType in getImmediateInterfaces(implementationType))
			{
				iface(interfaceType, implementationType);
			}
		}

		// http://stackoverflow.com/questions/5318685/get-only-direct-interface-instead-of-all

		static IEnumerable<Type> getImmediateInterfaces(Type implementationType)
		{
			var allInterfaces = implementationType.GetInterfaces();
			return
				allInterfaces.Except
					(allInterfaces.SelectMany(t => t.GetInterfaces()));
		}

		public IScope beginScope()
		{
#if DEBUG
			_frozen = true;
#endif
			return new LifetimeScope(this);
		}

	}
}
