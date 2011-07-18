using System;
using Newtonsoft.Json;

namespace ElFinder.Integration
{
	public abstract class ElFinderContent<T>
	{
		public ElFinderContent(T id)
		{
			Id = id;
		}

		[JsonIgnore]
		public T Id { get; set; }

		[JsonIgnore]
		internal Object Parent { get; set; }

		[JsonIgnore]
		public T IdParent
		{
			get
			{
				if (Parent is T)
					return (T)Parent;
				return default(T);
			}
			set { Parent = value; }
		}

		public String Hash { get { return Id.ToString(); } }

		public String Phash { get { return Parent == null ? String.Empty : Parent.ToString(); } }

		public String Name { get; set; }

		[JsonIgnore]
		public ElFinderRights Rights { get; set; }

		public int Read { get { return Rights.HasFlag(ElFinderRights.Read) ? 1 : 0; } }
		public int Write { get { return Rights.HasFlag(ElFinderRights.Write) ? 1 : 0; } }
		public int Rm { get { return Rights.HasFlag(ElFinderRights.Remove) ? 1 : 0; } }
	}
}