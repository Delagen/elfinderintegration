using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ElFinder.Integration
{
	public class MultipleFilesStoreExample : IElFinderStoreInterface<String>
	{
		private readonly IDictionary<String, IElFinderStoreInterface<String>> _controllers = new Dictionary<String, IElFinderStoreInterface<String>>();

		public MultipleFilesStoreExample()
		{
			_controllers.Add("Home", new FilesStoreExample("Home", "C:\\Temp"));
			_controllers.Add("Other", new FilesStoreExample("Other", "C:\\Temp2"));
		}

		private IElFinderStoreInterface<String> GetCurrentStoreInterface(string path)
		{
			var homeFolder = path.IndexOf(Path.DirectorySeparatorChar) >= 0 ? path.Substring(0, path.IndexOf(Path.DirectorySeparatorChar)) : path;
			if (_controllers.Keys.Contains(homeFolder))
				return _controllers[homeFolder];
			throw new ArgumentException("Отсутствующая корневая папка");
		}

		public IEnumerable<ElFinderContent<String>> ElGetList(String id, ElFinderListMode mode, out ElFinderWorkingDirectory<String> current)
		{
			var result = new List<ElFinderContent<string>>();
			ElFinderWorkingDirectory<String> tcurrent;

			var controller = String.IsNullOrEmpty(id) ? _controllers.Values.First() : GetCurrentStoreInterface(id.DecodePath());

			result.AddRange(controller.ElGetList(id, mode, out current));

			if (mode.HasFlag(ElFinderListMode.Root))
				result.AddRange(_controllers.Values.Where(v => !v.Equals(controller)).SelectMany(c => c.ElGetList(String.Empty, ElFinderListMode.Root, out tcurrent)));
			return result;
		}

		public ElFinderDirectory<String> ElMakeDirectory(ElFinderWorkingDirectory<String> current, string name)
		{
			var path = current.Id.DecodePath();
			var controller = GetCurrentStoreInterface(path);
			return controller.ElMakeDirectory(current, name);
		}

		public ElFinderFile<String> ElMakeFile(ElFinderWorkingDirectory<String> current, string name)
		{
			throw new NotImplementedException();
		}

		public ElFinderContent<String> ElRename(ElFinderContent<String> target, string name)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ElFinderContent<String>> ElCopy(IEnumerable<ElFinderContent<String>> targets, ElFinderWorkingDirectory<String> destination, bool move)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ElFinderContent<String>> ElRemove(IEnumerable<ElFinderContent<String>> targets)
		{
			throw new NotImplementedException();
		}

		public Stream ElGetContent(ElFinderFile<String> file)
		{
			throw new NotImplementedException();
		}

		public ElFinderFile<String> ElSetContent(ElFinderFile<String> target, Stream content)
		{
			throw new NotImplementedException();
		}

		public ElFinderFile<String> ElResize(ElFinderFile<String> target, Size size)
		{
			throw new NotImplementedException();
		}

		public ElFinderFile<String> ElArchive(ElFinderWorkingDirectory<String> destination, String type, IEnumerable<ElFinderContent<String>> targets)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ElFinderContent<String>> ElExtract(ElFinderFile<String> file, ElFinderWorkingDirectory<String> destination)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ElFinderFile<String>> ElGetThumbnails(IEnumerable<ElFinderFile<String>> contents)
		{
			throw new NotImplementedException();
		}
	}
}
