using System;

namespace Konstruktor2.Detail
{
	interface IDisposeChain : IDisposable
	{
		void add(IDisposable disposable);
	}
}