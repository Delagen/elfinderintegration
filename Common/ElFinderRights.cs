using System;

namespace ElFinder.Integration
{
	[Flags]
	public enum ElFinderRights
	{
		None = 0x00,
		Read = 0x01,
		Write = 0x02,
		Remove = 0x04,
		All = 0xff
	}
}