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

		static readonly Action<object, string> Implementation = 
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
			var l = a.Length;
			for (var i = 0; i != l; ++i)
				yield return i;
		}
	}

	// http://stackoverflow.com/questions/786383/c-sharp-events-and-thread-safety

	static class EventExtensions
	{
		public static void raise(this Action action)
		{
			action?.Invoke();
		}

		public static void raise<T1>(this Action<T1> action, T1 value1)
		{
			action?.Invoke(value1);
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
			_action_?.Invoke();
		}
	}
}
