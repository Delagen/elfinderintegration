namespace ElFinder.Integration
{
	public enum ElFinderCommand
	{
		Open,
		Parents,
		Tree,

		MkDir,
		MkFile,
		Rename,
		Paste,
		Get,
		Put,
		Upload,

		File, //get file content & download = 1 to send download header

		Rm,

		Duplicate,
		Archive,
		Extract,
		Resize,
		Tmb,

		Search
	}
}