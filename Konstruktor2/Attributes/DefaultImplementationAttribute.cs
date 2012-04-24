using System;

namespace Konstruktor
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class DefaultImplementationAttribute : Attribute
	{
		public readonly Type[] InterfaceTypes;

		public DefaultImplementationAttribute(params Type[] interfaceTypes)
		{
			InterfaceTypes = interfaceTypes;
		}
	}
}
