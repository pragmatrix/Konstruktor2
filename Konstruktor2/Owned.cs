using System;

namespace Konstruktor2
{
	public sealed class Owned<T> : IDisposable
	{
		readonly IDisposable _owner;
		readonly T _value;

		public Owned(ILifetimeScope owner, T value)
			: this((IDisposable)owner, value)
		{
		}

		internal Owned(IDisposable owner, T value)
		{
			_owner = owner;
			_value = value;
		}

		public T Value
		{
			get { return _value; }
		}

		public void Dispose()
		{
			_owner.Dispose();
		}
	}

	public static class Owned
	{
		public static Owned<T> ownedBy<T>(this T value, IDisposable owner)
		{
			return new Owned<T>(owner, value);
		}
	}
}
