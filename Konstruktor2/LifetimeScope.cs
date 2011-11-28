using System;
using System.Collections.Generic;

namespace Konstruktor
{
	sealed class LifetimeScope : ILifetimeScope
	{
		readonly object _ = new object();
		readonly ILifetimeScope _parent_;
		readonly IBuilder _builder;
		readonly uint _level;

		readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
		readonly IList<IDisposable> _objectsToDispose = new List<IDisposable>();

		public LifetimeScope(IBuilder builder)
		{
			_builder = builder;
		}

		LifetimeScope(IBuilder builder, ILifetimeScope parent, uint level)
			:this(builder)
		{
			_parent_ = parent;
			_level = level;

			// store the scope itself,
			// this enables Owned<T> to work without hacks
			
			_instances.Add(typeof (ILifetimeScope), this);
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

		public object resolve(Type type)
		{
			lock (_)
			{
				object instance_;
				if (internalTryResolveExisting(type, out instance_))
					return instance_;

				var newObj = _builder.build(type, this);

				internalStore(type, newObj);
				return newObj;
			}
		}

		public bool tryResolveExisting(Type type, out object o)
		{
			lock (_)
			{
				return internalTryResolveExisting(type, out o);
			}
		}

		public object resolveLocal(Type type)
		{
			lock (_)
			{
				object o;
				if (_instances.TryGetValue(type, out o))
					return o;

				var newObj = _builder.build(type, this);
				internalStore(type, newObj);
				return newObj;
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
			var disp = instance as IDisposable;
			if (disp != null)
				_objectsToDispose.Add(disp);
		}

		public ILifetimeScope beginNestedScope()
		{
			return new LifetimeScope(_builder, this, _level+1);
		}

		#endregion

		bool internalTryResolveExisting(Type type, out object instance)
		{
			if (_instances.TryGetValue(type, out instance))
				return true;

			if (_parent_ == null)
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
			return GetType().Name + " " + _level + " " + String.Format("{0,8:X}", (uint)GetHashCode());
		}
	}
}
