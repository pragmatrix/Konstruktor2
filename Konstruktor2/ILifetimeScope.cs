using System;

namespace Konstruktor
{
	public interface ILifetimeScope : IDisposable
	{

		// Resolve in this scope or parent scopes, 
		// create a new instance in this scope if it is not existing yet.

		object resolve(Type t, bool askParent=true);
		
		void store<TypeT>(TypeT instance);
		void own(object instance_);

		ILifetimeScope beginNestedScope();

		// internal
		bool tryResolveExisting(Type t, out object o);
	}

	public static class LifetimeScopeExtensions
	{
		public static T resolve<T>(this ILifetimeScope lifetimeScope, bool askParent=true)
		{
			return (T) lifetimeScope.resolve(typeof (T), askParent);
		}
	}
}
