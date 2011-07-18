using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace ElFinder.Integration
{
	public class FilesStoreExample : IElFinderStoreInterface<String>
	{
		private string DecodePath(string path)
		{
			return Encoding.Default.GetString(Enumerable.Range(0, path.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(path.Substring(x, 2), 16)).ToArray());
			//return Encoding.Default.GetString(Convert.FromBase64String(path.Replace('-', '+').Replace('_', '/').Replace('.', '=')));
		}
		private string EncodePath(string path)
		{
			return BitConverter.ToString(Encoding.Default.GetBytes(path)).Replace("-", String.Empty);
		}

		private string _workDir = "C:\\Temp";
		private string _workDirName = "Home";

		private ElFinderFile<String> GetEntry(String rootDirName, String rootDirPath, FileInfo info)
		{
			var parent = info.Directory.FullName.Replace(rootDirPath, String.Empty);
			if (parent.StartsWith(Path.DirectorySeparatorChar.ToString()))
				parent = parent.Remove(0, 1);
			return new ElFinderFile<string>(EncodePath(Path.Combine(rootDirName, parent, info.Name)))
					{
						IdParent = String.IsNullOrEmpty(parent) ? EncodePath(rootDirName) : EncodePath(Path.Combine(rootDirName, parent)),
						Name = info.Name,
						DateTime = info.LastWriteTime,
						Mime = "text/plain",
						Rights = ElFinderRights.Read//для тестов
					};
		}

		private ElFinderDirectory<String> GetEntry(String rootDirName, String rootDirPath, DirectoryInfo info)
		{
			var parent = info.FullName.Equals(rootDirPath) ? String.Empty : info.Parent.FullName.Replace(rootDirPath, String.Empty);
			if (parent.StartsWith(Path.DirectorySeparatorChar.ToString()))
				parent = parent.Remove(0, 1);
			var result = new ElFinderDirectory<string>(EncodePath(info.FullName.Equals(rootDirPath) ? rootDirName : Path.Combine(rootDirName, parent, info.Name)))
			{
				Name = info.FullName.Equals(rootDirPath) ? rootDirName : info.Name,
				DateTime = info.LastWriteTime,
				Rights = ElFinderRights.Read//для тестов
			};
			if (!info.FullName.Equals(rootDirPath))
				result.IdParent = String.IsNullOrEmpty(parent) ? EncodePath(rootDirName) : EncodePath(Path.Combine(rootDirName, parent));
			return result;
		}

		private string TranslatePath(String rootDirName, String rootDirPath, string relPath)
		{
			var path = relPath.Remove(0, _workDirName.Length);
			if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
				path = path.Remove(0, 1);
			return Path.Combine(_workDir, path);
		}

		public IEnumerable<ElFinderContent<string>> ElGetList(string id, ElFinderListMode mode, out ElFinderWorkingDirectory<string> current)
		{
			var result = new List<ElFinderContent<string>>();


			//папка по умолчанию обязательна для указания в качестве текущей
			current = new ElFinderWorkingDirectory<string>(new ElFinderDirectory<string>(EncodePath(_workDirName))
			{
				Name = _workDirName,
				DateTime = DateTime.Now,
				Rights = ElFinderRights.Read
			}, _workDirName, new Uri("/files/", UriKind.Relative), new Uri("/files/tmb/", UriKind.Relative));

			if (!String.IsNullOrEmpty(id))
			{
				var relPath = DecodePath(id);
				if (relPath.StartsWith(_workDirName))//определение корневой папки в случае если их несколько в принципе можно реализовать класс, который будет реализовывать подключение нужных IElFinderStoreInterface в зависимости от папки, реализация думаю будет не сложна
				{
					var path = TranslatePath(_workDirName, _workDir, relPath);

					//определение текущей папки для вывода
					current = new ElFinderWorkingDirectory<string>(GetEntry(_workDirName, _workDir, new DirectoryInfo(path)), relPath, new Uri("/files/", UriKind.Relative), new Uri("/files/tmb/", UriKind.Relative));

					if (mode.HasFlag(ElFinderListMode.Children))//вывести список папки
					{
						result.AddRange(Directory.GetDirectories(path).Select(d => GetEntry(_workDirName, _workDir, new DirectoryInfo(d))));
						result.AddRange(Directory.GetFiles(path).Select(d => GetEntry(_workDirName, _workDir, new FileInfo(d))));
					}

					if (mode.HasFlag(ElFinderListMode.Parents))//вывод указанной папки и родительских для нее
					{
						result.Add(GetEntry(_workDirName, _workDir, new DirectoryInfo(path)));
						if (!path.Equals(_workDir))
							result.Add(GetEntry(_workDirName, _workDir, new DirectoryInfo(path).Parent));
					}

					if (mode.HasFlag(ElFinderListMode.ParentsToRoot))//вывести все родительские папки вплоть до корневой, иначе будет запрос через Parents
					{
					}
				}
			}
			if (mode.HasFlag(ElFinderListMode.Root))//вывести корневые папки
			{
				if (!result.Any(c => c.Id.Equals(EncodePath(_workDirName))))//не выводить повторно если уже выведена
					result.Add(GetEntry(_workDirName, _workDir, new DirectoryInfo(_workDir)));
			}
			return result;
		}

		public ElFinderDirectory<string> ElMakeDirectory(ElFinderWorkingDirectory<string> current, string name)
		{
			var path = DecodePath(current.Id);
			if (path.StartsWith(_workDirName))//определение корневой папки в случае если их несколько в принципе можно реализовать класс, который будет реализовывать подключение нужных IElFinderStoreInterface в зависимости от папки, реализация думаю будет не сложна
			{
				path = Path.Combine(TranslatePath(_workDirName, _workDir, path), name);
				//! todo проверка прав и проверка существования
				return GetEntry(_workDirName, _workDir, Directory.CreateDirectory(path));
			}
			throw new ArgumentException("Неопределена корневая директория");
		}

		public ElFinderFile<string> ElMakeFile(ElFinderWorkingDirectory<string> current, string name)
		{
			throw new NotImplementedException();
		}

		public ElFinderContent<string> ElRename(ElFinderContent<string> target, string name)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ElFinderContent<string>> ElCopy(IEnumerable<ElFinderContent<string>> targets, ElFinderWorkingDirectory<string> destination, bool move)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ElFinderContent<string>> ElRemove(IEnumerable<ElFinderContent<string>> targets)
		{
			throw new NotImplementedException();
		}

		public Stream ElGetContent(ElFinderFile<string> file)
		{
			throw new NotImplementedException();
		}

		public ElFinderFile<string> ElSetContent(ElFinderFile<string> target, Stream content)
		{
			throw new NotImplementedException();
		}

		public ElFinderFile<string> ElResize(ElFinderFile<string> target, Size size)
		{
			throw new NotImplementedException();
		}

		public ElFinderFile<string> ElArchive(ElFinderWorkingDirectory<string> destination, string type, IEnumerable<ElFinderContent<string>> targets)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ElFinderContent<string>> ElExtract(ElFinderFile<string> file, ElFinderWorkingDirectory<string> destination)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ElFinderFile<string>> ElGetThumbnails(IEnumerable<ElFinderFile<string>> contents)
		{
			throw new NotImplementedException();
		}
	}
}
