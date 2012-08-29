using System;
using System.Diagnostics;
using Konstruktor2.Detail;

namespace Konstruktor2
{
	public class Activator<ParamT, ResultT> : IDisposable
		where ResultT : class
	{
		readonly Func<ParamT, Owned<ResultT>> _generator;
		Owned<ResultT> _generated_;
		ParamT _param;

		public ParamT Param
		{
			get
			{
				Debug.Assert(IsActive);
				return _param;
			}
		}

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

		public virtual void Dispose()
		{
			if (IsActive)
				deactivate();
		}

		public void activate(ParamT param)
		{
			if (IsActive)
				deactivate();

			if (_disablingRequests != 0)
				return;

			_param = param;
			_generated_ = _generator(param);

			activated(param);

			Activated.raise(param);
			ActivationChanged.raise();
		}


		public void deactivate()
		{
			if (_generated_ == null)
				return;

			Deactivating.raise();

			deactivating();

			_generated_.Dispose();
			_generated_ = null;
			_param = default(ParamT);

			ActivationChanged.raise();
		}

		protected virtual void activated(ParamT param)
		{
		}

		protected virtual void deactivating()
		{
		}

		public event Action<ParamT> Activated;
		public event Action Deactivating;

		public event Action ActivationChanged;

		int _disablingRequests;

		public IDisposable beginDisable()
		{
			if (IsActive)
				deactivate();

			++_disablingRequests;

			return new DisposeAction(() => --_disablingRequests);
		}
	}
}
