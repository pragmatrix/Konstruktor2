using NUnit.Framework;

namespace Konstruktor2.Tests
{
	[TestFixture]
	sealed class ImplementationOnly
	{
		sealed class Dummy2
		{}

		sealed class Dummy
		{
			public readonly Dummy2 _d2;
			public Dummy(Dummy2 d2)
			{
				_d2 = d2;
			}
		}

		[Test]
		public void testImplementationInstantiation()
		{
			var b = new Konstruktor();

			using (var scope = b.beginScope())
			{
				var dummy = scope.resolve<Dummy>();
				var dummy2 = scope.resolve<Dummy2>();

				Assert.AreEqual(dummy._d2, dummy2);
			}
		}
	}
}
