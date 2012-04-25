using System;

namespace Konstruktor
{
	/*
		A pinned attribute instructs Konstruktor to instantiate the attribted class 
		or interface alonside an other class's instance.

		This mechanism makes it possible to create lifetime scopes that contain more 
		than one root-class.

		For example, if documents are created by a factory method Func<Document> and 
		each of these documents require (but do not refer) a number of services local 
		to their lifetime scopes, these class could be pinned to the Document class.
	*/

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
	public sealed class PinnedToAttribute : Attribute
	{
		public readonly Type TargetType;

		public PinnedToAttribute(Type targetType)
		{
			TargetType = targetType;
		}
	}
}
