using NUnit.Framework;

namespace Konstruktor.Tests
{
	[TestFixture]
	sealed class Store
	{
		sealed class A
		{
		}
		
		sealed class B
		{
			public readonly A A;
			public B(A a)
			{
				A = a;
			}
		}
		

		[Test]
		public void explicitZeroStore()
		{
			var b = new Konstruktor();
	
			using (var scope = b.beginScope())
			{
				scope.store<A>(null);
				var br = scope.resolve<B>();
				Assert.AreEqual(null, br.A);
			}
		}
	}
}
