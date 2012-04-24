using System;

namespace Konstruktor.Detail
{
	interface IDisposeChain : IDisposable
	{
		void add(IDisposable disposable);
	}
}