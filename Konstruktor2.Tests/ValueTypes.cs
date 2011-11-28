using System;
using NUnit.Framework;

namespace Konstruktor.Tests
{
	[TestFixture]
	sealed class ValueTypes
	{
		sealed class RefersValueType
		{
			public RefersValueType(char value)
			{
				throw new NotImplementedException();
			}
		}

		sealed class RefersString
		{
			public RefersString(string value)
			{
				throw new NotImplementedException();
			}
		}

		[Test, ExpectedException(typeof(ResolveException))]
		public void testValueTypes()
		{
			var b = new Builder();

			using (var s = b.beginScope())
			{
				s.resolve<RefersValueType>();
			}
		}

		[Test, ExpectedException(typeof(ResolveException))]
		public void testStringType()
		{
			var b = new Builder();

			using (var s = b.beginScope())
			{
				s.resolve<RefersString>();
			}
		}
	}
}
