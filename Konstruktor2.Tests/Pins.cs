using System.Reflection;
using NUnit.Framework;

namespace Konstruktor2.Tests
{

	[TestFixture]
	public sealed class Pins
	{
		Konstruktor k;

		[SetUp]
		public void setup()
		{
			k = new Konstruktor();
			k.scanAssembly(Assembly.GetExecutingAssembly());
		}

		[Test]
		public void pinClassToClass()
		{
			_pinnedInstantiated = false;

			using (var s = k.beginScope<Document>())
			{
				var d = s.resolve<Document>();
				Assert.That(_pinnedInstantiated, Is.True);
				var p = s.resolve<PinnedToDocument>();
				Assert.That(p._doc, Is.EqualTo(d));
			}
		}

		public class Document
		{
		}

		static bool _pinnedInstantiated = false;

		[PinnedTo(typeof(Document))]
		public class PinnedToDocument
		{
			public readonly Document _doc;

			public PinnedToDocument(Document doc)
			{
				_pinnedInstantiated = true;
				_doc = doc;
			}
		}

		[Test]
		public void pinInterfaceToClass()
		{
			_pinned2Instantiated = false;

			using (k.beginScope<Document2>())
			{
				Assert.That(_pinned2Instantiated, Is.True);
			}
		}

		public class Document2
		{
		}

		[PinnedTo(typeof(Document2))]
		public interface IPinned
		{
		}

		static bool _pinned2Instantiated = false;


		[DefaultImplementation]
		public class PinnedToDocument2Impl : IPinned
		{
			public PinnedToDocument2Impl()
			{
				_pinned2Instantiated = true;
			}
		}


		[Test]
		public void pinClassToInterface()
		{
			_pinned3Instantiated = false;

			using (var s = k.beginScope<IDocument3>())
			{
				Assert.That(_pinned3Instantiated, Is.True);
			}

		}

		public interface IDocument3
		{
		}

		[DefaultImplementation]
		sealed class Document3 : IDocument3
		{
		}

		[PinnedTo(typeof(IDocument3))]
		public sealed class PinnedToDocument3
		{
			public PinnedToDocument3()
			{
				_pinned3Instantiated = true;
			}
		}

		static bool _pinned3Instantiated;


		/*
			Pinned4 is resolved in the topmost scope and RootDocument4 is the root of the nested scope.
			This should result in Pinned4 to be reinstantiated in the nested scope.
		*/

		[Test]
		public void pinDerivedInNestedScope()
		{
			using (var rootScope = k.beginScope())
			{
				var p1 = rootScope.resolve<Pinned4>();

				using (var nestedScope = rootScope.beginNestedScope<RootDocument4>())
				{
					var p2 = nestedScope.resolve<Pinned4>();
					Assert.That(p1 != p2);
				}
			}
		}

		sealed class RootDocument4
		{
		}


		[PinnedTo(typeof(RootDocument4))]
		sealed class Pinned4
		{
			public Pinned4()
			{
			}
		}
	}
}
