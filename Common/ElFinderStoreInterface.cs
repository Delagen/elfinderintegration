using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace ElFinder.Integration
{
	public interface IElFinderStoreInterface<T>
	{
		IEnumerable<ElFinderContent<T>> ElGetList(T id, ElFinderListMode mode, out ElFinderWorkingDirectory<T> current);

		ElFinderDirectory<T> ElMakeDirectory(ElFinderWorkingDirectory<T> current, String name);
		ElFinderFile<T> ElMakeFile(ElFinderWorkingDirectory<T> current, String name);

		ElFinderContent<T> ElRename(ElFinderContent<T> target, String name);
		IEnumerable<ElFinderContent<T>> ElCopy(IEnumerable<ElFinderContent<T>> targets, ElFinderWorkingDirectory<T> destination, Boolean move);
		IEnumerable<ElFinderContent<T>> ElRemove(IEnumerable<ElFinderContent<T>> targets);

		Stream ElGetContent(ElFinderFile<T> file);
		ElFinderFile<T> ElSetContent(ElFinderFile<T> target, Stream content);

		ElFinderFile<T> ElResize(ElFinderFile<T> target, Size size);

		ElFinderFile<T> ElArchive(ElFinderWorkingDirectory<T> destination, String type, IEnumerable<ElFinderContent<T>> targets);
		IEnumerable<ElFinderContent<T>> ElExtract(ElFinderFile<T> file, ElFinderWorkingDirectory<T> destination);


		IEnumerable<ElFinderFile<T>> ElGetThumbnails(IEnumerable<ElFinderFile<T>> contents);
	}

}
