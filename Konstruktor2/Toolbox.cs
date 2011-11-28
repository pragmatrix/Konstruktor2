using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Konstruktor
{
	public static class StringFormatExtensions
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

	public static class LogExtensions
	{
		[Conditional("DEBUG")]
		public static void Debug(this object obj, string msg)
		{
			Implementation(obj, msg);
		}

		public static Action<object, string> Implementation = 
			(obj, msg) => System.Diagnostics.Debug.WriteLine(obj.ToString() + ": " + msg);
	}

	public static class MemberInfoExtensions
	{
		public static bool hasAttribute<AttributeT>(this MemberInfo mi)
			where AttributeT : Attribute
		{
			return mi.IsDefined(typeof(AttributeT), false);
		}
	}

	public static class Indices
	{
		public static IEnumerable<int> indices(this Array a)
		{
			int l = a.Length;
			for (int i = 0; i != l; ++i)
				yield return i;
		}
	}
}
