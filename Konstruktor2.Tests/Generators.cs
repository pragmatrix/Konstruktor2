using NUnit.Framework;

namespace Konstruktor.Tests
{
	[TestFixture]
	sealed class Generators
	{
		interface IDummy
		{
		}

		class Implementation : IDummy {}

		[Test]
		public void testExplicitGenerator()
		{
			var builder = new Konstruktor();
			builder.registerGenerator<IDummy>(s => new Implementation());

			using (var scope = builder.beginScope())
			{
				var dummy = scope.resolve<IDummy>();
				Assert.AreEqual(typeof(Implementation), dummy.GetType());
			}
		}

		sealed class Implementation2 : IDummy
		{ }

		[Test]
		public void testExplicitGeneratorPrecedence()
		{
			var builder = new Konstruktor();
			builder.forInterface<IDummy>().generate(s => new Implementation());
			builder.forInterface<IDummy>().instantiate<Implementation2>();

			using (var scope = builder.beginScope())
			{
				var dummy = scope.resolve<IDummy>();
				Assert.AreEqual(typeof(Implementation), dummy.GetType());
			}
		}

		[Test]
		public void testImplementationGenerator()
		{
			var builder = new Konstruktor();
			builder.registerGenerator(s => new Implementation());

			using (var scope = builder.beginScope())
			{
				var dummy = scope.resolve<Implementation>();
				Assert.AreEqual(typeof(Implementation), dummy.GetType());
			}
		}
	}
}
