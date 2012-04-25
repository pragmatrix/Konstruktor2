using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Konstruktor2.Tests
{
	[TestFixture]
	sealed class Lifetime
	{
		static readonly List<object> _disposed = new List<object>();


		interface IB { }
		sealed class B : IB, IDisposable
		{
			public void Dispose()
			{
				_disposed.Add(this);
			}
		}


		sealed class A : IDisposable
		{
			readonly B b;
			public A(B b)
			{
				this.b = b;
			}


			public void Dispose()
			{
				_disposed.Add(this);
			}
		}


		[Test]
		public  void inOrderDispose()
		{
			_disposed.Clear();

			var b = new Konstruktor();

			using (var scope = b.beginScope())
			{
				scope.resolve<A>();
			}

			Assert.AreEqual(2, _disposed.Count);
			Assert.AreEqual(typeof(A), _disposed[0].GetType());
			Assert.AreEqual(typeof(B), _disposed[1].GetType());
		}

		[Test]
		public void interfaceDispose()
		{
			_disposed.Clear();

			var b = new Konstruktor();

			b.forInterface<IB>().instantiate<B>();

			using (var scope = b.beginScope())
			{
				// both should be the same object, but
				// dispose should be only called on A's instance (once).

				var b1 = scope.resolve<IB>();
				var b2 = scope.resolve<B>();

				Assert.AreSame(b1, b2);
			}

			Assert.AreEqual(1, _disposed.Count);
			Assert.AreEqual(typeof(B), _disposed[0].GetType());
		}
	}
}
