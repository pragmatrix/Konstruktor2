using System;
using System.Collections.Generic;

namespace Konstruktor2.Detail
{
	sealed class LifetimeScope : ILifetimeScope
	{
		readonly object _ = new object();
		Type _root_;
		readonly ILifetimeScope _parent_;
		readonly IKonstruktor _konstruktor;
		public uint Level { get; }

		readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
		readonly IList<IDisposable> _objectsToDispose = new List<IDisposable>();

		public LifetimeScope(IKonstruktor konstruktor, ILifetimeScope parent_)
		{
			_konstruktor = konstruktor;
			_parent_ = parent_;
			if (_parent_ != null)
				Level = _parent_.Level + 1;

			// store the scope itself,
			// this enables Owned<T> to work without hacks
			
			_instances.Add(typeof (ILifetimeScope), this);
		}

		bool isRootOrPinned(Type t)
		{
			return t == _root_ || _root_ != null && _konstruktor.isPinnedTo(t, _root_);
		}

		#region Public / Thread Safe

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
					if (_objectsToDispose.Count == 0)
						break;
					var lastIndex = _objectsToDispose.Count - 1;
					disp = _objectsToDispose[lastIndex];
					_objectsToDispose.RemoveAt(lastIndex);
				}

				disp.Dispose();
			} 

			// now we are safe!
			_instances.Clear();
		}

		/*
			We were tempted to put the root type in the constructor of the lifetime scope, but
			then we can not provide arguments for the root instances.
		*/

		public object resolveRoot(Type type)
		{
			lock (_)
			{
				if (_root_ != null)
					throw new ResolveException("Root type has already been resolved", type);

				_root_ = type;

				// force all instances that are pinned to the root type to resolve.

				foreach (var pin in _konstruktor.pinsOf(_root_))
					resolve(pin);

				return resolve(_root_);
			}
		}

		public object resolve(Type type)
		{
			lock (_)
			{
				if (internalTryResolveExisting(type, out var instance_))
					return instance_;

				var newInstance = _konstruktor.build(this, type);
				internalStore(type, newInstance);

				return newInstance;
			}
		}

		public bool tryResolveExisting(Type type, out object o)
		{
			lock (_)
			{
				return internalTryResolveExisting(type, out o);
			}
		}
		
		public void store<TypeT>(TypeT instance)
		{
			lock (_)
				internalStore(typeof(TypeT), instance);
		}

		public void own(object instance_)
		{
			if (instance_ == null)
				return;

			lock (_)
			{
				internalOwn(instance_);
			}
		}

		void internalOwn(object instance)
		{
			if (instance is IDisposable disp)
				_objectsToDispose.Add(disp);
		}

		public ILifetimeScope beginNestedScope()
		{
			return new LifetimeScope(_konstruktor, this);
		}

		#endregion

		bool internalTryResolveExisting(Type type, out object instance)
		{
			if (_instances.TryGetValue(type, out instance))
				return true;

			if (_parent_ == null || isRootOrPinned(type))
				return false;

			return _parent_.tryResolveExisting(type, out instance);
		}

		void internalStore(Type type, object instance_)
		{
			_instances.Add(type, instance_);
			// this.Debug("stor {2,8:X}: {0} => {1}".format(type.Name, instance_, instance_ != null ? (uint)instance_.GetHashCode() : 0));
		}

		public override string ToString()
		{
			return GetType().Name + " " + Level + " " + String.Format("{0,8:X}", (uint)GetHashCode());
		}
	}
}
