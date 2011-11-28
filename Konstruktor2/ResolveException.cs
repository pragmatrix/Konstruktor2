using System;

namespace Konstruktor
{
	public sealed class ResolveException : Exception
	{
		public ResolveException(string msg, Type t)
			: base(msg.fmt(t))
		{
		}
	}
}
