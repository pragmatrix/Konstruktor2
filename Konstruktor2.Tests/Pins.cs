using System.Reflection;
using NUnit.Framework;

namespace Konstruktor.Tests
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
			using (var s = k.beginScope())
			{
				_pinnedInstantiated = false;

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

		[PinTo(typeof(Document))]
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
			using (var s = k.beginScope())
			{
				_pinned2Instantiated = false;

				s.resolve<Document2>();
				Assert.That(_pinned2Instantiated, Is.True);
			}
		}

		public class Document2
		{
		}

		[PinTo(typeof(Document2))]
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
			using (var s = k.beginScope())
			{
				_pinned3Instantiated = false;

				var d = s.resolve<IDocument3>();
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

		[PinTo(typeof(IDocument3))]
		public sealed class PinnedToDocument3
		{
			public PinnedToDocument3()
			{
				_pinned3Instantiated = true;
			}


		}

		static bool _pinned3Instantiated;
	}

}
