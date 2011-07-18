using System;

namespace ElFinder.Integration
{
	public class ElFinderWorkingDirectory<T> : ElFinderDirectory<T>
	{
		public ElFinderWorkingDirectory(ElFinderDirectory<T> directory, String path, Uri thumbUrl, Uri filesUrl)
			: base(directory.Id)
		{
			Parent = directory.Parent;
			DateTime = directory.DateTime;
			Name = directory.Name;
			Rights = directory.Rights;

			Path = path;
			ThumbUrl = thumbUrl;
			FilesUrl = filesUrl;
		}

		public String Path { get; private set; }
		public Uri ThumbUrl { get; private set; }
		public Uri FilesUrl { get; private set; }
	}
}