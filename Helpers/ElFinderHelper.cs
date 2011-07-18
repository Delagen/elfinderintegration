using System;
using System.Collections.Generic;

namespace ElFinder.Integration
{
	internal static class ElFinderHelper
	{
		public static Boolean IsTrue(this int? option)
		{
			return option.HasValue && option.Value == 1;
		}

		public static Dictionary<TK, TV> AddIf<TK, TV>(this Dictionary<TK, TV> data, Func<Boolean> condition, Func<KeyValuePair<TK, TV>> result)
			where TK : class
			where TV : class
		{
			return AddIf(data, condition, () => new[] { result() });
		}

		public static Dictionary<TK, TV> AddIf<TK, TV>(this Dictionary<TK, TV> data, Func<Boolean> condition, Func<IEnumerable<KeyValuePair<TK, TV>>> result)
			where TK : class
			where TV : class
		{
			if (condition())
			{
				foreach (var item in result())
					data.Add(item.Key, item.Value);
			}
			return data;
		}
	}
}