using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Konstruktor.Tests
{
	[TestFixture]
	sealed class Examples
	{
		#region myFirstKonstrukt

		sealed class A
		{
			public void printHelloWorld()
			{
				Console.WriteLine("Hello World");
			}
		}

		sealed class B
		{
			public readonly A A;
			public B(A a)
			{
				A = a;
			}
		}

		[Test]
		public static void myFirstKonstrukt()
		{
			var b = new Builder();
			using (var s = b.beginScope())
			{
				var bInstance = s.resolve<B>();
				bInstance.A.printHelloWorld();
			}
		}

		#endregion

		#region Map Interface

		interface IInterface
		{
		}
		
		sealed class Implementation : IInterface
		{
		}

		[Test]
		public static void mapInterface()
		{
			var b = new Builder();
			b.forInterface<IInterface>().instantiate<Implementation>();
			using (var s = b.beginScope())
			{
				var implementation = s.resolve<IInterface>();
				Assert.AreEqual(typeof(Implementation), implementation.GetType());
			}
		}

		#endregion

		#region Func

		sealed class ClientInstance
		{
			public readonly Server Server;

			public ClientInstance(Server server)
			{
				Server = server;
			}
		}

		sealed class Server
		{
			readonly Func<ClientInstance> _instanceCreator;

			public Server(Func<ClientInstance> instanceCreator)
			{
				_instanceCreator = instanceCreator;
			}

			public ClientInstance createClient()
			{
				return _instanceCreator();
			}
		}

		[Test]
		public static void serverFunc()
		{
			var b = new Builder();
			using (var s = b.beginScope())
			{
				var server = s.resolve<Server>();
				var client1 = server.createClient();
				var client2 = server.createClient();

				Assert.AreNotSame(client1, client2);
				Assert.AreSame(client1.Server, server);
				Assert.AreSame(client2.Server, server);
			}
		}

		#endregion

		interface IGeneratorExample
		{}

		sealed class GeneratorExample : IGeneratorExample
		{
		}

		[Test]
		public static void explicitGenerator()
		{
			var b = new Builder();
			b.generator<IGeneratorExample>(scope => new GeneratorExample());
			using (var s = b.beginScope())
			{
				var generated = s.resolve<IGeneratorExample>();
				Assert.AreEqual(typeof (GeneratorExample), generated.GetType());
			}
		}

		sealed class PropertyInjection
		{
			public GeneratorExample Generator { get; set; }
		}

		[Test]
		public static void explicitGeneratorPropertyInjection()
		{
			var b = new Builder();
			b.generator(scope => new PropertyInjection {Generator = scope.resolve<GeneratorExample>()});

			using (var s = b.beginScope())
			{
				var generated = s.resolve<PropertyInjection>();
				Assert.AreEqual(typeof(PropertyInjection), generated.GetType());
				Assert.AreNotEqual(null, generated.Generator);
			}
		}
	}
}
