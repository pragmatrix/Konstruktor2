using System;

namespace Konstruktor2
{
	[AttributeUsage(AttributeTargets.Constructor)]
	public sealed class PreferredConstructorAttribute : Attribute
	{
	}
}
