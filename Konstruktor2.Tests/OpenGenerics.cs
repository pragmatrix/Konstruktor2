using NUnit.Framework;

namespace Konstruktor.Tests
{
	[TestFixture]
	public sealed class OpenGenerics
	{
		interface IShared<T>
		{
		}

		sealed class Shared<T> : IShared<T>
		{
		}

		[Test]
		public void openGeneric()
		{
			var builder = new Builder();
			builder.mapInterfaceToImplementation(typeof(IShared<>), typeof(Shared<>));

			using (var scope = builder.beginScope())
			{
				var shared = scope.resolve<IShared<int>>();
				Assert.AreEqual(typeof(Shared<int>), shared.GetType());
			}
		}

		interface IShared2<T, U>
		{
		}

		sealed class Shared2<T, U> : IShared2<T, U>
		{
		}

		[Test]
		public void openGeneric2()
		{
			var builder = new Builder();
			builder.mapInterfaceToImplementation(typeof(IShared2<,>), typeof(Shared2<,>));

			using (var scope = builder.beginScope())
			{
				var shared = scope.resolve<IShared2<int, double>>();
				var shared2 = scope.resolve<IShared2<int, float>>();
				Assert.AreEqual(typeof(Shared2<int, double>), shared.GetType());
				Assert.AreEqual(typeof(Shared2<int, float>), shared2.GetType());
			}
		}

		interface IOG<T>
		{
		}

		sealed class OG<T> : IOG<T>
		{
		}

		sealed class ExplicitInt : IOG<int>
		{
		}

		[Test]
		public void openGenericAndExplicit()
		{
			var builder = new Builder();
			builder.mapInterfaceToImplementation(typeof(IOG<>), typeof(OG<>));
			builder.mapInterfaceToImplementation(typeof(IOG<int>), typeof(ExplicitInt));

			using (var scope = builder.beginScope())
			{
				var fl = scope.resolve<IOG<float>>();
				var i = scope.resolve<IOG<int>>();

				Assert.AreEqual(typeof(OG<float>), fl.GetType());
				Assert.AreEqual(typeof(ExplicitInt), i.GetType());
			}
		}

		interface IShared2<T>
		{
		}

		sealed class Shared2<T> : IShared2<T>
		{
			T t;

			public Shared2(T t)
			{
				this.t = t;
			}
		}

		sealed class Dummy
		{}

		[Test]
		public void openGenericGenerator()
		{
			var builder = new Builder();
			builder.registerGeneratorsIn(GetType());

			using (var scope = builder.beginScope())
			{
				var shared = scope.resolve<IShared2<Dummy>>();
				Assert.AreEqual(typeof(Shared2<Dummy>), shared.GetType());

				var shared2 = scope.resolve<IShared2<Dummy>>();
				Assert.AreEqual(shared, shared2);
			}
		}

		[GeneratorMethod]
		static IShared2<T> generateShared<T>(T t)
		{
			return new Shared2<T>(t);
		}
	}
}
