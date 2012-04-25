using System;
using System.Collections.Generic;

namespace Konstruktor2.Detail
{
	interface IKonstruktor
	{
		object build(ILifetimeScope lifetimeScope, Type t);
		IEnumerable<Type> pinsOf(Type targetType);
		bool isPinnedTo(Type t, Type targetType);
	}
}