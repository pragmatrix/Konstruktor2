using System;

namespace Konstruktor2
{
	public interface ILifetimeScope : IDisposable
	{
		// Resolve roots and all pined instances that refer to it.
		object resolveRoot(Type t);

		// Resolve in this scope or parent scopes, 
		// create a new instance in this scope if it is not existing yet.

		object resolve(Type t);
		
		void store<TypeT>(TypeT instance);
		void own(object instance_);

		ILifetimeScope beginNestedScope();

		// internal

		bool tryResolveExisting(Type t, out object o);
		uint Level { get; }
	}

	public static class LifetimeScopeExtensions
	{
		public static T resolve<T>(this ILifetimeScope lifetimeScope)
		{
			return (T) lifetimeScope.resolve(typeof (T));
		}

		/// Try get an instance that already exists in the lifetime scope.

		public static bool tryGet<T>(this ILifetimeScope lifetimeScope, out T value)
		{
			if (lifetimeScope.tryResolveExisting(typeof (T), out var r))
			{
				value = (T) r;
				return true;
			}
			value = default(T);
			return false;
		}

		public static TypeT resolveRoot<TypeT>(this ILifetimeScope scope)
		{
			return (TypeT)scope.resolveRoot(typeof(TypeT));
		}
	}
}
