using System;
using Konstruktor.Detail;

namespace Konstruktor
{
	[Serializable]
	public sealed class ResolveException : Exception
	{
		public ResolveException(string msg, Type t)
			: base(msg.fmt(t))
		{
		}
	}
}
