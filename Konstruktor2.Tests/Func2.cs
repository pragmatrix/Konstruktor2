using System;
using NUnit.Framework;

namespace Konstruktor.Tests
{
	[TestFixture]
	sealed class Func2
	{

		sealed class Param
		{}

		static bool _instanceDestructed = false;

		sealed class Instance : IDisposable
		{
			public readonly Param P;

			public Instance(Param p)
			{
				P = p;
			}

			public void Dispose()
			{
				_instanceDestructed = true;
			}
		}

		sealed class Factory
		{
			public readonly Func<Param, Instance> _factory;

			public Factory(Func<Param, Instance> factory)
			{
				_factory = factory;
			}
		}

		[Test]
		public void testFunc2()
		{
			_instanceDestructed = false;

			var b = new Builder();

			using (var scope = b.beginScope())
			{
				var f = scope.resolve<Factory>();

				var p = new Param();
				var i = f._factory(p);
				Assert.AreEqual(i.P, p);
			}

			// sub-scopes do not dispose!, factory clients must handle that!

			Assert.AreEqual(false, _instanceDestructed);
		}
	}
}
