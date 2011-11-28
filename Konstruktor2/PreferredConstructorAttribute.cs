using System;

namespace Konstruktor
{
	[AttributeUsage(AttributeTargets.Constructor)]
	public sealed class PreferredConstructorAttribute : Attribute
	{
	}
}
