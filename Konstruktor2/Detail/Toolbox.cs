using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Konstruktor2.Detail
{
	static class StringFormatExtensions
	{
		public static string fmt(this string format, params object[] objects)
		{
			// very important: if there is no formatting intended, use the format literally.
			// Otherwise output that contains {} may break formatting.

			if (objects.Length == 0)
				return format;

			try
			{
				return string.Format(format, objects);
			}
			catch (Exception e)
			{
				return e.Message;
			}
		}
	}

	static class LogExtensions
	{
		[Conditional("DEBUG")]
		public static void Debug(this object obj, string msg)
		{
			Implementation(obj, msg);
		}

		public static Action<object, string> Implementation = 
			(obj, msg) => System.Diagnostics.Debug.WriteLine(obj.ToString() + ": " + msg);
	}

	static class MemberInfoExtensions
	{
		public static bool hasAttribute<AttributeT>(this MemberInfo mi)
			where AttributeT : Attribute
		{
			return mi.IsDefined(typeof(AttributeT), false);
		}
	}

	static class Indices
	{
		public static IEnumerable<int> indices(this Array a)
		{
			int l = a.Length;
			for (int i = 0; i != l; ++i)
				yield return i;
		}
	}

	// http://stackoverflow.com/questions/786383/c-sharp-events-and-thread-safety

	static class EventExtensions
	{
		public static void raise(this Action action)
		{
			if (action != null)
				action();
		}

		public static void raise<T1>(this Action<T1> action, T1 value1)
		{
			if (action != null)
				action(value1);
		}

		public static void raise<T1, T2>(this Action<T1, T2> action, T1 value1, T2 value2)
		{
			if (action != null)
				action(value1, value2);
		}
	}

	sealed class DisposeAction : IDisposable
	{
		readonly Action _action_;

		public DisposeAction(Action action)
		{
			Debug.Assert(action != null);
			_action_ = action;
		}

		public void Dispose()
		{
			if (_action_ != null)
				_action_();
		}

		// that should be faster than an empty DisposeAction 
		// (because boxing to IDisposable would always create a new instance).

		public static readonly IDisposable None = new DummyDisposable();

		sealed class DummyDisposable : IDisposable
		{
			public void Dispose()
			{
			}
		}
	}
}
