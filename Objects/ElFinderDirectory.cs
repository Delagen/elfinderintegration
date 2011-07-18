using System;
using Newtonsoft.Json;

namespace ElFinder.Integration
{
	public class ElFinderDirectory<T> : ElFinderContent<T>
	{
		public ElFinderDirectory(T id):base(id)
		{
		}
			
		[JsonIgnore]
		public DateTime DateTime { get; set; }

		public int Dirs { get { return 1; } }

		public String Date { get { return DateTime.ToShortDateString(); } }

		public string Mime
		{
			get { return "directory"; }
		}
	}
}