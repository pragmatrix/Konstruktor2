using System;
using System.Diagnostics;

namespace Konstruktor2.Detail
{
	static class Func1Factory
	{
		public static object instantiate(Type t, ILifetimeScope lifetimeScope)
		{
			Debug.Assert(t.GetGenericTypeDefinition() == typeof(Func<>));
			var funcArgs = t.GetGenericArguments();
			Debug.Assert(funcArgs.Length == 1);

			var resultType = funcArgs[0];
	
			var typeDef = typeof(Func1Factory<>);
			var factoryType = typeDef.MakeGenericType( resultType);

			var factoryInstance = (IFunc1Factory)Activator.CreateInstance(factoryType, lifetimeScope);
			return factoryInstance.resolveFactoryMethod();
		}
	}

	interface IFunc1Factory
	{
		object resolveFactoryMethod();
	}

	sealed class Func1Factory<ResultT> : IFunc1Factory
	{
		readonly ILifetimeScope _lifetimeScope;

		public Func1Factory(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
		}

		public object resolveFactoryMethod()
		{
			Func<ResultT> method = () =>
			{
				this.Debug("");
				
				var nested = _lifetimeScope.beginNestedScope();
				try
				{
					return nested.resolveRoot<ResultT>();
				}
				catch (Exception)
				{
					nested.Dispose();
					throw;
				}
				
			};

			return method;
		}
	}
}
