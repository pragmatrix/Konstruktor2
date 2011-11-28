using System;
using NUnit.Framework;

namespace Konstruktor.Tests
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
			var b = new Builder();

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
			var b = new Builder();

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
			var b = new Builder();

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
			var b = new Builder();

			using (var s = b.beginScope())
			{
				s.resolve<Preferred>();
			}
		}

		#endregion

		#region Preferred by Builder

		sealed class PreferredByBuilder
		{
			public PreferredByBuilder(B b)
			{
				
			}

			public PreferredByBuilder(B b, B c)
			{
				throw new NotImplementedException();
			}
		}


		[Test]
		public void preferredByBuilder()
		{
			var b = new Builder();

			b.preferConstructor<PreferredByBuilder>(typeof(B));

			using (var s = b.beginScope())
			{
				s.resolve<PreferredByBuilder>();
			}
		}

		#endregion
	}
}
