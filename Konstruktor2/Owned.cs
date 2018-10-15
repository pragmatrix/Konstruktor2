using System;

namespace Konstruktor2
{
	public sealed class Owned<T> : IDisposable
	{
		readonly IDisposable _owner;

		public Owned(ILifetimeScope owner, T value)
			: this((IDisposable)owner, value)
		{
		}

		internal Owned(IDisposable owner, T value)
		{
			_owner = owner;
			Value = value;
		}

		public T Value { get; }

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
