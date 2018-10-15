using Konstruktor2.Detail;

namespace Konstruktor2
{
	// TBD: Check if this is required anymore for iOS AOT compilation.
	public static class AOT
	{
		public static void Func2<ArgT, ResultT>()
		{
			// ReSharper disable once ObjectCreationAsStatement
			new Func2Factory<ArgT, ResultT>(null);
			LifetimeScope ls = null;
			// ReSharper disable once PossibleNullReferenceException
			ls.store(default(ArgT));
		}
	}
}
