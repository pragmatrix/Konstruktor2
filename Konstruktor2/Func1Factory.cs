using System;
using System.Diagnostics;

namespace Konstruktor
{
	static class Func1Factory
	{
		public static object instantiate(Type t, IScope scope)
		{
			Debug.Assert(t.GetGenericTypeDefinition() == typeof(Func<>));
			var funcArgs = t.GetGenericArguments();
			Debug.Assert(funcArgs.Length == 1);

			var resultType = funcArgs[0];
	
			var typeDef = typeof(Func1Factory<>);
			var factoryType = typeDef.MakeGenericType( resultType);

			var factoryInstance = (IFunc1Factory)Activator.CreateInstance(factoryType, scope);
			return factoryInstance.resolveFactoryMethod();
		}
	}

	interface IFunc1Factory
	{
		object resolveFactoryMethod();
	}

	sealed class Func1Factory<ResultT> : IFunc1Factory
	{
		readonly IScope _scope;

		public Func1Factory(IScope scope)
		{
			_scope = scope;
		}

		public object resolveFactoryMethod()
		{
			Func<ResultT> method = () =>
			{
				this.Debug("");
				
				var nested = _scope.beginNestedScope();
				return nested.resolveLocal<ResultT>();
			};

			return method;
		}
	}
}
