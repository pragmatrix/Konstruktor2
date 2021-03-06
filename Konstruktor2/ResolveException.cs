﻿using System;
using Konstruktor2.Detail;

namespace Konstruktor2
{
	public sealed class ResolveException : Exception
	{
		public ResolveException(string msg, Type t)
			: base(msg.fmt(t))
		{
		}
	}
}
