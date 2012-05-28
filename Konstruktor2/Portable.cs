namespace System.Collections.Generic
{
	sealed class HashSet<T> : IEnumerable<T>
	{
		struct Empty
		{
			public static readonly Empty Instance;
		}

		readonly Dictionary<T, Empty> _dict = new Dictionary<T, Empty>();

		public bool Add(T value)
		{
			if (_dict.ContainsKey(value))
				return false;
			_dict.Add(value, Empty.Instance);
			return true;
		}

		#region IEnumerable<T>

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _dict.Keys.GetEnumerator();
		}

		#endregion
	}
}
