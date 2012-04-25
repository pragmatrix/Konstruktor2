using System;
using NUnit.Framework;

namespace Konstruktor2.Tests
{
	[TestFixture]
	sealed class ConstructorPrecendence
	{
		sealed class B
		{}

		sealed class Ambiguous
		{
			public Ambiguous(int a)
			{
				throw new NotImplementedException();
			}

			public Ambiguous(B b)
			{
				throw new NotImplementedException();
			}
		}

		[Test, ExpectedException(typeof(ResolveException))]
		public void ambiguous()
		{
			var b = new Konstruktor();

			using (var s = b.beginScope())
			{
				s.resolve<Ambiguous>();
			}
		}

		sealed class InternalConstructor
		{
			internal InternalConstructor(B b)
			{
			}
		}

		[Test, ExpectedException(typeof(ResolveException))]
		public void internalConstructor()
		{
			var b = new Konstruktor();

			using (var s = b.beginScope())
			{
				s.resolve<InternalConstructor>();
			}
		}

		sealed class MoreArguments
		{
			public MoreArguments(B b)
			{
				throw new NotImplementedException();
			}

			public MoreArguments(B b, B c)
			{
			}
		}

		[Test]
		public void moreArguments()
		{
			var b = new Konstruktor();

			using (var s = b.beginScope())
			{
				s.resolve<MoreArguments>();
			}
		}

		#region Preferred by Attribute

		sealed class Preferred
		{
			[PreferredConstructor]
			public Preferred(B b)
			{
			}

			public Preferred(B b, B c)
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		public void preferred()
		{
			var b = new Konstruktor();

			using (var s = b.beginScope())
			{
				s.resolve<Preferred>();
			}
		}

		#endregion

		#region Preferred by Konstruktor

		sealed class PreferredByKonstruktor
		{
			public PreferredByKonstruktor(B b)
			{
				
			}

			public PreferredByKonstruktor(B b, B c)
			{
				throw new NotImplementedException();
			}
		}


		[Test]
		public void preferredByBuilder()
		{
			var b = new Konstruktor();

			b.preferConstructor<PreferredByKonstruktor>(typeof(B));

			using (var s = b.beginScope())
			{
				s.resolve<PreferredByKonstruktor>();
			}
		}

		#endregion
	}
}
