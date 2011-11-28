using System.Diagnostics;
using NUnit.Framework;

namespace Konstruktor.Tests
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
			var builder = new Builder();
			builder.forInterface<IDummy>().use<Implementation>();

			using (var scope = builder.beginScope())
			{
				var dummy = scope.resolve<IDummy>();
				Debug.Assert(dummy.GetType() == typeof(Implementation));

				var dummy2 = scope.resolve<IDummy>();
				Debug.Assert(ReferenceEquals(dummy, dummy2));
			}
		}

		[Test, ExpectedException(typeof(ResolveException))]
		public void testImplementationSecondaryUnregistered()
		{
			var builder = new Builder();
			builder.forInterface<IDummy>().use<Implementation>();

			using (var scope = builder.beginScope())
			{
				var dummy = scope.resolve<IDummy>();
				var dummy2 = scope.resolve<ISecondary>();
				Assert.True(ReferenceEquals(dummy, dummy2));
			}
		}

		[Test]
		public void testImplementationSecondaryRegistered()
		{
			var builder = new Builder();
			builder.forInterface<IDummy>().use<Implementation>();
			builder.forInterface<ISecondary>().use<Implementation>();

			using (var scope = builder.beginScope())
			{
				var dummy = scope.resolve<IDummy>();
				var dummy2 = scope.resolve<ISecondary>();
				Assert.True(ReferenceEquals(dummy, dummy2));
			}
		}
	}
}
