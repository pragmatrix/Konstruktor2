namespace Konstruktor
{
	public static class KonstruktorExtensions
	{
		public static ILifetimeScope beginScope<RootT>(this Konstruktor konstruktor)
		{
			var scope = konstruktor.beginScope();
			scope.resolveRoot(typeof (RootT));
			return scope;
		}
	}
}
