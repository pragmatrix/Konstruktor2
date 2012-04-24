using System;
using System.Diagnostics;

namespace Konstruktor.Detail
{
	static class Func1Factory
	{
		public static object instantiate(Type t, IKonstruktorScope konstruktorScope)
		{
			Debug.Assert(t.GetGenericTypeDefinition() == typeof(Func<>));
			var funcArgs = t.GetGenericArguments();
			Debug.Assert(funcArgs.Length == 1);

			var resultType = funcArgs[0];
	
			var typeDef = typeof(Func1Factory<>);
			var factoryType = typeDef.MakeGenericType( resultType);

			var factoryInstance = (IFunc1Factory)Activator.CreateInstance(factoryType, konstruktorScope);
			return factoryInstance.resolveFactoryMethod();
		}
	}

	interface IFunc1Factory
	{
		object resolveFactoryMethod();
	}

	sealed class Func1Factory<ResultT> : IFunc1Factory
	{
		readonly IKonstruktorScope _konstruktorScope;

		public Func1Factory(IKonstruktorScope konstruktorScope)
		{
			_konstruktorScope = konstruktorScope;
		}

		public object resolveFactoryMethod()
		{
			Func<ResultT> method = () =>
			{
				this.Debug("");
				
				var nested = _konstruktorScope.beginNestedScope();
				return nested.resolveLocal<ResultT>();
			};

			return method;
		}
	}
}
