using System;
using System.Diagnostics;

namespace Konstruktor2
{
	public class Activator<ParamT, ResultT> : IDisposable
		where ResultT : class
	{
		readonly Func<ParamT, Owned<ResultT>> _generator;
		Owned<ResultT> _generated_;

		public ResultT Instance_
		{
			get { return IsActive ? _generated_.Value : null; }
		}

		public bool IsActive
		{
			get { return _generated_ != null; }
		}

		public Activator(Func<ParamT, Owned<ResultT>> generator)
		{
			_generator = generator;
		}

		public void Dispose()
		{
			if (IsActive)
				deactivate();
		}

		public virtual void activate(ParamT param)
		{
			if (IsActive)
				deactivate();

			_generated_ = _generator(param);
		}

		public virtual void deactivate()
		{
			Debug.Assert(_generated_ != null);
			_generated_.Dispose();
			_generated_ = null;
		}
	}
}
