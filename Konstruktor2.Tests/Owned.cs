using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Konstruktor.Tests
{
	[TestFixture]
	sealed class Owned
	{

		sealed class NestedInstanceReferred : IDisposable
		{
			public bool Disposed;

			public void Dispose()
			{
				Debug.Assert(!Disposed);
				Disposed = true;
			}
		}

		sealed class NestedInstance : IDisposable
		{
			public NestedInstanceReferred Referred;

			public NestedInstance(NestedInstanceReferred referred)
			{
				Referred = referred;
			}

			public bool Disposed;

			public void Dispose()
			{
				Debug.Assert(!Disposed);
				Disposed = true;
			}
		}

		sealed class OwnedFactory
		{
			readonly Func<Owned<NestedInstance>> _factory;

			public OwnedFactory(Func<Owned<NestedInstance>> factory)
			{
				_factory = factory;
			}

			public Owned<NestedInstance> build()
			{
				return _factory();
			}
		}


		[Test]
		public void nestedScopeOwned()
		{
			var b = new Konstruktor();

			using (var scope = b.beginScope())
			{
				var referrer = scope.resolve<OwnedFactory>();

				Owned<NestedInstance> inst;

				using (inst = referrer.build())
				{
				}

				Assert.True(inst.Value.Disposed);
				Assert.True(inst.Value.Referred.Disposed);
			}
		}
	}
}
