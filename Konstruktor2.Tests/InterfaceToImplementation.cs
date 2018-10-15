using System.Diagnostics;
using NUnit.Framework;

namespace Konstruktor2.Tests
{
	[TestFixture]
	public sealed class InterfaceToImplementation
	{
		interface IDummy
		{ }

		interface ISecondary
		{
		}

		sealed class Implementation : IDummy, ISecondary
		{
		};

		[Test]
		public void testConcrete()
		{
			var builder = new Konstruktor();
			builder.forInterface<IDummy>().instantiate<Implementation>();

			using (var scope = builder.beginScope())
			{
				var dummy = scope.resolve<IDummy>();
				Debug.Assert(dummy.GetType() == typeof(Implementation));

				var dummy2 = scope.resolve<IDummy>();
				Debug.Assert(ReferenceEquals(dummy, dummy2));
			}
		}

		[Test]
		public void testImplementationSecondaryUnregistered()
		{
			var builder = new Konstruktor();
			builder.forInterface<IDummy>().instantiate<Implementation>();

			using (var scope = builder.beginScope())
			{
				var dummy = scope.resolve<IDummy>();
				Assert.That(() => scope.resolve<ISecondary>(), Throws.TypeOf<ResolveException>());
			}
		}

		[Test]
		public void testImplementationSecondaryRegistered()
		{
			var builder = new Konstruktor();
			builder.forInterface<IDummy>().instantiate<Implementation>();
			builder.forInterface<ISecondary>().instantiate<Implementation>();

			using (var scope = builder.beginScope())
			{
				var dummy = scope.resolve<IDummy>();
				var dummy2 = scope.resolve<ISecondary>();
				Assert.True(ReferenceEquals(dummy, dummy2));
			}
		}
	}
}
