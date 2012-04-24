using System;

namespace Konstruktor
{
	public interface ILifetimeScope : IDisposable
	{

		// Resolve in this scope or parent scopes, create a new instance in this scope if it is not existing yet.
		object resolve(Type t);

		// Resolve in this scope: create a new instance that does not exist in this scope (ignoring the ancestor scopes).
		// Dependencies are resolved by asking ancestor scopes, though.
		object resolveLocal(Type t);
		
		void store<TypeT>(TypeT instance);
		void own(object instance_);

		ILifetimeScope beginNestedScope();

		// internal
		bool tryResolveExisting(Type t, out object o);
	}

	public static class ScopeExtensions
	{
		public static T resolve<T>(this ILifetimeScope lifetimeScope)
		{
			return (T) lifetimeScope.resolve(typeof (T));
		}

		public static T resolveLocal<T>(this ILifetimeScope lifetimeScope)
		{
			return (T) lifetimeScope.resolveLocal(typeof (T));
		}
	}
}
