using System.Reflection;
using NUnit.Framework;

namespace Konstruktor.Tests
{
	[TestFixture]
	public sealed class DefaultImplementation
	{
		interface InterfaceBase
		{}

		interface Interface1 : InterfaceBase
		{}

		[DefaultImplementation]
		sealed class MyDefault : Interface1
		{
		};

		Konstruktor b;

		[SetUp]
		public void setup()
		{
			b = new Konstruktor();
			b.scanAssembly(Assembly.GetExecutingAssembly());
		}

		[Test]
		public void testIfInterface1IsRegistered()
		{
			using (var scope = b.beginScope())
			{
				scope.resolve<Interface1>();
			}
		}

		[Test, ExpectedException(typeof(ResolveException))]
		public void testIfOnlyInterface1IsRegistered()
		{
			using (var scope = b.beginScope())
			{
				scope.resolve<InterfaceBase>();
			}
		}

		interface Interface2 : InterfaceBase
		{ }

		interface Interface3 : InterfaceBase
		{ }

		[DefaultImplementation(typeof(Interface3))]
		sealed class MyDefault2 : Interface3, Interface2
		{ }

		[Test]
		public void testIfSpecificInterfaceIsRegistered()
		{
			using (var scope = b.beginScope())
			{
				scope.resolve<Interface3>();
			}
		}

		[Test, ExpectedException(typeof(ResolveException))]
		public void testIfOnlySpecificInterfaceIsRegistered()
		{
			using (var scope = b.beginScope())
			{
				scope.resolve<Interface2>();
			}
		}

		interface Interface4
		{}

		interface Interface5
		{}

		interface Interface6
		{}

		[DefaultImplementation(typeof(Interface4))]
		[DefaultImplementation(typeof(Interface5))]
		sealed class MyDefault3 : Interface4, Interface5, Interface6
		{}

		[Test]
		public void testIfMultipleInterfacesAreRegistered()
		{
			using (var scope = b.beginScope())
			{
				var x = scope.resolve<Interface4>();
				var y = scope.resolve<Interface5>();
				Assert.AreSame(x, y);
			}
		}

		[Test, ExpectedException(typeof(ResolveException))]
		public void testIfOnlySpecifiedMultipleInterfacesAreRegistered()
		{
			using (var scope = b.beginScope())
			{
				scope.resolve<Interface6>();
			}
		}

		interface Interface7
		{ }

		interface Interface8
		{ }

		interface Interface9
		{ }

		[DefaultImplementation(typeof(Interface7), typeof(Interface8))]
		sealed class MyDefault4 : Interface7, Interface8, Interface9
		{ }

		[Test]
		public void testIfMultipleInterfacesAreRegisteredParam()
		{
			using (var scope = b.beginScope())
			{
				var x = scope.resolve<Interface7>();
				var y = scope.resolve<Interface8>();
				Assert.AreSame(x, y);
			}
		}

		[Test, ExpectedException(typeof(ResolveException))]
		public void testIfOnlySpecifiedMultipleInterfacesAreRegisteredParam()
		{
			using (var scope = b.beginScope())
			{
				scope.resolve<Interface9>();
			}
		}
	}
}
