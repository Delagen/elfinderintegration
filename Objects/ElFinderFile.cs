using System;
using Newtonsoft.Json;

namespace ElFinder.Integration
{
	public class ElFinderFile<T> : ElFinderContent<T>
	{
		public ElFinderFile(T id):base(id)
		{
		}

		[JsonIgnore]
		public DateTime DateTime { get; set; }

		public String Date { get { return DateTime.ToShortDateString(); } }

		public String Tmb { get; set; }

		public Int64 Size { get; set; }

		public string Mime { get; set; }

	}
}