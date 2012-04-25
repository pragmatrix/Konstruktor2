using System;

namespace Konstruktor
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

		public static TypeT resolveRoot<TypeT>(this ILifetimeScope scope)
		{
			return (TypeT)scope.resolveRoot(typeof(TypeT));
		}

		public static ILifetimeScope beginNestedScope<RootT>(this ILifetimeScope lifetimeScope)
		{
			var scope = lifetimeScope.beginNestedScope();
			scope.resolveRoot<RootT>();
			return scope;
		}
	}
}
