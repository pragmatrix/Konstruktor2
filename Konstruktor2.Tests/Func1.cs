using System;
using NUnit.Framework;

namespace Konstruktor.Tests
{
	[TestFixture]
	sealed class Func1
	{

		static uint _instancesDestructed = 0;

		sealed class Instance : IDisposable
		{
			public void Dispose()
			{
				++_instancesDestructed;
			}
		}

		sealed class Factory
		{
			public readonly Func<Instance> _factory;

			public Factory(Func<Instance> factory)
			{
				_factory = factory;
			}
		}

		[Test]
		public void testFunc1()
		{
			_instancesDestructed = 0;

			var b = new Konstruktor();

			using (var scope = b.beginScope())
			{
				var f = scope.resolve<Factory>();
				var fi = scope.resolve<Instance>();

				var i = f._factory();

				Assert.AreNotSame(fi, i);
			}

			// sub-scopes do not dispose!, factory clients must handle that!

			Assert.AreEqual(1, _instancesDestructed);
		}
	}
}
