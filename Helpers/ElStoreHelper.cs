using System.Collections.Generic;

namespace ElFinder.Integration
{
	internal static class ElStoreHelper
	{
		public static IEnumerable<ElFinderContent<T>> ElGetList<T>(this IElFinderStoreInterface<T> store, T id, ElFinderListMode mode)
		{
			ElFinderWorkingDirectory<T> content;
			return store.ElGetList(id, mode, out content);
		}

		public static IEnumerable<ElFinderContent<T>> ElGetList<T>(this IElFinderStoreInterface<T> store, T id)
		{
			ElFinderWorkingDirectory<T> content;
			return store.ElGetList(id, ElFinderListMode.Info, out content);
		}

		public static bool HasItems<T>(this IList<T> container)
		{
			return container != null && container.Count > 0;
		}
	}
}