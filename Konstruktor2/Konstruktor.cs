using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Konstruktor2.Detail;

namespace Konstruktor2
{
	public sealed partial class Konstruktor : IKonstruktor
	{
		readonly Dictionary<Type, Func<ILifetimeScope, object>> _explicitGenerators = new Dictionary<Type, Func<ILifetimeScope, object>>();
		readonly Dictionary<Type, Type> _interfaceToImplementation = new Dictionary<Type, Type>();
		readonly Dictionary<Type, MethodInfo> _generatorMethods = new Dictionary<Type, MethodInfo>();
		readonly Dictionary<Type, Type[]> _preferredConstructor = new Dictionary<Type, Type[]>();
		readonly Dictionary<Type, HashSet<Type>> _pins = new Dictionary<Type, HashSet<Type>>(); 

#if DEBUG
		bool _frozen;
#endif

		public ForInterface<InterfaceT> forInterface<InterfaceT>()
			where InterfaceT:class
		{
			return new ForInterface<InterfaceT>(this);
		}

		public sealed class ForInterface<InterfaceT>
			where InterfaceT:class
		{
			readonly Konstruktor _konstruktor;

			public ForInterface(Konstruktor konstruktor)
			{
				_konstruktor = konstruktor;
			}

			public void instantiate<ImplementationT>()
				where ImplementationT : InterfaceT
			{
				_konstruktor.mapInterfaceToImplementation(typeof (InterfaceT), typeof (ImplementationT));
			}

			public void generate(Func<ILifetimeScope, InterfaceT> generator)
			{
				_konstruktor.registerGenerator(generator);
			}
		}

		public void mapInterfaceToImplementation(Type interfaceType, Type implementationType)
		{
#if DEBUG
			Debug.Assert(!_frozen);
#endif

			var interfaceTypeInfo = interfaceType.GetTypeInfo();
			var implementationTypeInfo = implementationType.GetTypeInfo();

			if (!interfaceTypeInfo.IsInterface)
				throw new ArgumentException("must be an interface", nameof(interfaceType));

			if (implementationTypeInfo.IsInterface)
				throw new ArgumentException("must be an implementation type", nameof(implementationType));

			if (interfaceTypeInfo.IsGenericTypeDefinition != implementationTypeInfo.IsGenericTypeDefinition)
				throw new ArgumentException("none or both must be open generic types", "interfaceType, implementationType");

			if (interfaceTypeInfo.IsGenericTypeDefinition && interfaceTypeInfo.GetGenericArguments().Length != implementationTypeInfo.GetGenericArguments().Length)
				throw new ArgumentException("number of generic arguments do not match", "interfaceType, implementationType");

			if (!interfaceTypeInfo.IsGenericTypeDefinition && !interfaceTypeInfo.IsAssignableFrom(implementationType))
				throw new ArgumentException("interface is not implemented by the implementation", "interfaceType, implementationType");

			if (_interfaceToImplementation.ContainsKey(interfaceType))
				this.Debug("Reregistering implementation type {0} for interface {1}".fmt(implementationType.Name, interfaceType.Name));

			_interfaceToImplementation[interfaceType] = implementationType;
		}

		public void registerGenerator<GeneratedT>(Func<ILifetimeScope, GeneratedT> constructor)
		{
			_explicitGenerators.Add(typeof(GeneratedT), scope => constructor(scope));
		}

		public void registerGeneratorsIn<FactoryT>()
		{
			registerGeneratorsIn(typeof(FactoryT));
		}

		public void registerGeneratorsIn(Type type)
		{
#if DEBUG
			Debug.Assert(!_frozen);
#endif
			var typeInfo = type.GetTypeInfo();
			var methods = typeInfo.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			var factoryMethods = from m in methods where m.hasAttribute<GeneratorMethodAttribute>() select m;

			foreach (var fm in factoryMethods)
			{
				var returnType = fm.ReturnType;
				var returnTypeInfo = returnType.GetTypeInfo();
				var typeDefinition = returnTypeInfo.IsGenericType ? returnType.GetGenericTypeDefinition() : returnType;
				_generatorMethods.Add(typeDefinition, fm);
			}
		}

		public void preferConstructor<TypeT>(params Type[] constructorTypes)
		{
			_preferredConstructor[typeof (TypeT)] = constructorTypes;
		}

		public void scanAssembly(Assembly assembly)
		{
			foreach (var implementationType in assembly.GetTypes())
			{
				var implementationTypeInfo = implementationType.GetTypeInfo();

				var defImplementationAttributes = implementationTypeInfo.GetCustomAttributes(false);
				foreach (var attr in defImplementationAttributes)
				{
					switch (attr)
					{
						case DefaultImplementationAttribute defaultImplementation:
							registerDefaultImplementationAttribute(implementationType, defaultImplementation.InterfaceTypes);
							break;
						case PinnedToAttribute pinToAttribute:
							pinTo(implementationType, pinToAttribute.TargetType);
							break;
					}
				}
			}
		}

		void registerDefaultImplementationAttribute(Type implementationType, Type[] interfaces)
		{
			if (interfaces.Length == 0)
			{
				registerImplementation(implementationType);
				return;
			}

			foreach (var interfaceType in interfaces)
			{
				var interfaceTypeInfo = interfaceType.GetTypeInfo();
				Debug.Assert(interfaceTypeInfo.IsAssignableFrom(implementationType));
				mapInterfaceToImplementation(interfaceType, implementationType);
			}
		}

		void registerImplementation(Type implementationType)
		{
			foreach (var interfaceType in getImmediateInterfaces(implementationType))
			{
				mapInterfaceToImplementation(interfaceType, implementationType);
			}
		}

		void pinTo(Type pinnedType, Type targetType)
		{
			if (!_pins.TryGetValue(targetType, out var pinnedTypes))
				_pins.Add(targetType, pinnedTypes = new HashSet<Type>());
			pinnedTypes.Add(pinnedType);
		}

		IEnumerable<Type> IKonstruktor.pinsOf(Type targetType)
		{
			return _pins.TryGetValue(targetType, out var pinnedTypes) 
				? pinnedTypes 
				: Enumerable.Empty<Type>();
		}

		bool IKonstruktor.isPinnedTo(Type t, Type targetType)
		{
			return _pins.TryGetValue(targetType, out var pins) && pins.Contains(t);
		}

		// http://stackoverflow.com/questions/5318685/get-only-direct-interface-instead-of-all

		static IEnumerable<Type> getImmediateInterfaces(Type implementationType)
		{
			var implementationTypeInfo = implementationType.GetTypeInfo();
			var allInterfaces = implementationTypeInfo.GetInterfaces();
			return
				allInterfaces.Except
					(allInterfaces.SelectMany(t => t.GetTypeInfo().GetInterfaces()));
		}

		public ILifetimeScope beginScope()
		{
#if DEBUG
			_frozen = true;
#endif
			return new LifetimeScope(this, null);
		}
	}
}
