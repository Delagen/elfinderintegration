using System;

namespace ElFinder.Integration
{
	[Flags]
	public enum ElFinderListMode
	{
		Info = 0x00,
		Children = 0x01,//list of folder
		Parents = 0x02,//parent entries

		ParentsToRoot = 0x04,//parent entries
		Root = 0x08,//root entries
		All = 0xff
	}
}