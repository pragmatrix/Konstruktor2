using System;
using System.Collections.Generic;

namespace Konstruktor2.Detail
{
	sealed class DisposeChain : IDisposeChain
	{
		readonly object _ = new object();
		readonly List<IDisposable> _disposables = new List<IDisposable>();

		public DisposeChain(IDisposeChain parent_)
		{
			parent_?.add(this);
		}

		public void Dispose()
		{
			// while there are disposables active, we must assume that 
			// _anything_ can happen.

			// also we must assume that Dispose in this instance may be called
			// reentrant (in owned Owned<T>, for example).

			for (; ; )
			{
				IDisposable disp;
				lock (_)
				{
					if (_disposables.Count == 0)
						break;
					var lastIndex = _disposables.Count - 1;
					disp = _disposables[lastIndex];
					_disposables.RemoveAt(lastIndex);
				}

				disp.Dispose();
			} 
		}
 
		public void add(IDisposable disposable)
		{
			lock (_)
				_disposables.Add(disposable);
		}

	}
}
