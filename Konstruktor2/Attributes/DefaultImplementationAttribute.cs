using System;

namespace Konstruktor
{
	/*
		A class that is attributed wuth a DefaultImplementationAttribute is 
		registered by scanAssembly() as the default implementation for all 
		its immediate interfaces (the interfaces that are implemented by 
		the class but not its base classes).

		Optionally, the interfaces can be specified explicitly by passing them
		to the attribute. If one or more interfaces are explicitly specified,
		the immediate interfaces are ignored.
	*/

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
