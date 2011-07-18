using System;
using System.Linq;
using System.Text;

namespace ElFinder.Integration
{
	static class ElStringHelper
	{
		public static string DecodePath(this string path)
		{
			return Encoding.Default.GetString(Enumerable.Range(0, path.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(path.Substring(x, 2), 16)).ToArray());
		}

		public static string EncodePath(this string path)
		{
			return BitConverter.ToString(Encoding.Default.GetBytes(path)).Replace("-", String.Empty);
		}
	}
}
