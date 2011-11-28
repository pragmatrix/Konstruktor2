using NUnit.Framework;

namespace Konstruktor.Tests
{
	[TestFixture]
	sealed class ConstructorArguments
	{
		sealed class I2
		{}

		sealed class Instance
		{
			public readonly string Str;
			public readonly string Str2;

			public Instance(string str, I2 i2, string str2)
			{
				Str = str;
				Str2 = str2;
			}
		}


		[Test]
		public void test()
		{
			var b = new Builder();
			b.constructorArgument<Instance, string>(0, s => "S1");
			b.constructorArgument<Instance, string>(2, s => "S2");

		
			using (var scope = b.beginScope())
			{
				var i = scope.resolve<Instance>();

				Assert.AreEqual("S1", i.Str);
				Assert.AreEqual("S2", i.Str2);
			}
		}
	}
}
