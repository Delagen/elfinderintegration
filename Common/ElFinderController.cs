using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace ElFinder.Integration
{
	public class ElFinderController<TI, TSI> where TSI : IElFinderStoreInterface<TI>, new()
	{
		private readonly TSI _store;

		public ElFinderController()
		{
			_store = new TSI();
		}

		#region Response Elements
		private KeyValuePair<String, Object> ElError(string error)
		{
			return new KeyValuePair<string, object>("error", error);
		}

		private KeyValuePair<String, Object> ElApi(float version)
		{
			return new KeyValuePair<string, object>("api", version.ToString("F1", new NumberFormatInfo { NumberDecimalSeparator = "." }));
		}

		private KeyValuePair<String, Object> ElUploadMaxSize(string uplMaxSize = "16M")
		{
			return new KeyValuePair<string, object>("uplMaxSize", uplMaxSize);
		}

		private KeyValuePair<String, Object> ElOptions(ElFinderWorkingDirectory<TI> current, IEnumerable<String> createArchives, IEnumerable<String> extractArchives)
		{
			return new KeyValuePair<string, object>("options", new Dictionary<String, Object>
			                                                   	{
			                                                   		{"archives", new Dictionary<String, Object> {{"create", createArchives}, {"extract", extractArchives}}},
			                                                   		{"copyOverwrite", true},
			                                                   		{"disabled", new string[] {}},
			                                                   		{"path",current.Path},
			                                                   		{"separator", "/"},
			                                                   		{"tmbUrl", current.ThumbUrl},
			                                                   		{"url", current.FilesUrl}
			                                                   	});
		}

		private KeyValuePair<String, Object> ElCurrent(ElFinderContent<TI> content)
		{
			return new KeyValuePair<String, Object>("cwd", content);
		}

		private KeyValuePair<String, Object> ElTree(IEnumerable<ElFinderContent<TI>> files)
		{
			return new KeyValuePair<String, Object>("tree", files);
		}

		private KeyValuePair<String, Object> ElFiles(IEnumerable<ElFinderContent<TI>> files)
		{
			return new KeyValuePair<String, Object>("files", files);
		}

		private KeyValuePair<String, Object> ElAdded(IEnumerable<ElFinderContent<TI>> files)
		{
			return new KeyValuePair<String, Object>("added", files);
		}

		private KeyValuePair<String, Object> ElChanged(IEnumerable<ElFinderContent<TI>> files)
		{
			return new KeyValuePair<String, Object>("changed", files);
		}

		private KeyValuePair<String, Object> ElRemoved(IEnumerable<TI> files)
		{
			return new KeyValuePair<String, Object>("removed", files);
		}

		private KeyValuePair<String, Object> ElContent(String content)
		{
			return new KeyValuePair<String, Object>("content", content);
		}

		private IEnumerable<KeyValuePair<String, Object>> ElImages(IEnumerable<ElFinderFile<TI>> files)
		{
			return new[]
		            {
		                new KeyValuePair<String, Object>("images", files.Where(f=>!String.IsNullOrEmpty(f.Tmb)).ToDictionary(k=>k.Id,v=>v.Name))
		            };
		}

		#endregion


		public IDictionary<String, Object> ProcessCommand(ElFinderCommand cmd, int? init, int? tree, IList<TI> target, String name, IList<TI> src, IList<TI> dst, IList<TI> targets, int? cut, int? download, String type, Int32? width, Int32? height)
		{
			var data = new Dictionary<String, Object>();
			try
			{
				ElFinderWorkingDirectory<TI> current;

				switch (cmd)
				{
					case ElFinderCommand.Open:
						{
							var files = _store.ElGetList(target.FirstOrDefault(), ElFinderListMode.Info | (tree.IsTrue() ? ElFinderListMode.All : ElFinderListMode.Children), out current).ToList();
							data
								.AddIf(() => init.IsTrue(), () => new[] { ElApi(2), ElUploadMaxSize() })
								.AddIf(() => true, () => ElOptions(current, new string[] { }, new string[] { }))
								.AddIf(() => true, () => ElCurrent(current))
								.AddIf(() => true, () => ElFiles(files));
							break;
						}
					case ElFinderCommand.Tree:
						if (target.HasItems())
							data.AddIf(() => true, () => ElTree(_store.ElGetList(target.First(), ElFinderListMode.Children)));

						break;
					case ElFinderCommand.Parents:
						if (target.HasItems())
							data.AddIf(() => true, () => ElTree(_store.ElGetList(target.First(), ElFinderListMode.Parents)));
						break;
					case ElFinderCommand.MkDir:
						if (target.HasItems())
						{
							_store.ElGetList(target.First(), ElFinderListMode.Info, out current);
							if (!String.IsNullOrEmpty(name))
								data.AddIf(() => true, () => ElAdded(new[] { _store.ElMakeDirectory(current, name) }));
						}
						break;
					case ElFinderCommand.MkFile:
						if (target.HasItems())
						{
							_store.ElGetList(target.First(), ElFinderListMode.Info, out current);
							if (!String.IsNullOrEmpty(name))
								data.AddIf(() => true, () => ElAdded(new[] { _store.ElMakeFile(current, name) }));
						}
						break;

					case ElFinderCommand.Rename:
						if (target.HasItems())
						{
							var info = _store.ElGetList(target.First(), ElFinderListMode.Info, out current).FirstOrDefault();
							if (!String.IsNullOrEmpty(name) && info != null)
							{
								data
									.AddIf(() => true, () => ElAdded(new[] { _store.ElRename(info, name) }))
									.AddIf(() => true, () => ElRemoved(new[] { info.Id }));

							}
						}
						break;
					case ElFinderCommand.Paste:
						{
							if (target.HasItems() && src.HasItems() && dst.HasItems() && targets.HasItems() && cut.HasValue)
							{
								ElFinderWorkingDirectory<TI> from, to;
								var selectedFiles = _store.ElGetList(src.First(), ElFinderListMode.Children, out from);
								_store.ElGetList(dst.First(), ElFinderListMode.Info, out to);
								if (from != null && to != null)
								{
									var files = selectedFiles.Where(sf => targets.Contains(sf.Id)).ToList();
									data
										.AddIf(() => true, () => ElAdded(_store.ElCopy(files, to, cut.IsTrue())))
										.AddIf(() => true, () => ElRemoved(files.Select(f => f.Id)));
								}
							}
						}
						break;
					case ElFinderCommand.Get:
						if (target.HasItems())
						{
							var item = _store.ElGetList(target.First()).OfType<ElFinderFile<TI>>().FirstOrDefault();
							if (item != null)
							{
								using (var ms = new MemoryStream())
								{
									_store.ElGetContent(item).CopyTo(ms);
									ms.Seek(0, SeekOrigin.Begin);
									data.AddIf(() => true, () => ElContent(Encoding.Default.GetString(ms.ToArray())));
								}
							}

						}
						break;
					case ElFinderCommand.File:
						if (target.HasItems())
						{
							var item = _store.ElGetList(target.First()).OfType<ElFinderFile<TI>>().FirstOrDefault();
							if (item != null)
							{
								var stream = _store.ElGetContent(item);
								if (download.IsTrue())
								{
									HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment; filename=" + item.Name);
									HttpContext.Current.Response.AppendHeader("Content-Length", stream.Length.ToString());
								}
								HttpContext.Current.Response.ContentType = item.Mime;
								stream.CopyTo(HttpContext.Current.Response.OutputStream);
								HttpContext.Current.Response.End();
							}

						}
						break;
					case ElFinderCommand.Rm:
						if (targets.HasItems())
							data.AddIf(() => true, () => ElRemoved(_store.ElRemove(targets.Select(i => _store.ElGetList(i).FirstOrDefault()).Where(i => i != null)).Select(i => i.Id)));
						break;
					case ElFinderCommand.Resize:
						if (target.HasItems() && width.HasValue && height.HasValue)
						{
							var item = _store.ElGetList(target.First()).OfType<ElFinderFile<TI>>().FirstOrDefault();
							data.AddIf(() => true, () => ElChanged(new[] { _store.ElResize(item, new Size(width.Value, height.Value)) }));
						}
						break;

					case ElFinderCommand.Archive:
						//current type=application/x-tar name=Archive targets[]
						if (targets.HasItems() && targets.Count > 0 && !String.IsNullOrEmpty(type))
						{
							_store.ElGetList(targets.First(), ElFinderListMode.Info, out current);
							data.AddIf(() => true, () => ElAdded(new[] { _store.ElArchive(current, type, targets.Select(i => _store.ElGetList(i).FirstOrDefault())) }));
						}
						break;
					case ElFinderCommand.Extract:
						if (target.HasItems())
						{
							var item = _store.ElGetList(target.First(), ElFinderListMode.Info, out current).OfType<ElFinderFile<TI>>().FirstOrDefault();
							if (item != null)
								data.AddIf(() => true, () => ElAdded(_store.ElExtract(item, current)));
						}
						break;
					case ElFinderCommand.Tmb:
						if (targets.HasItems())
						{
							var list = targets.Select(i => _store.ElGetList(i, ElFinderListMode.Info, out current).OfType<ElFinderFile<TI>>().FirstOrDefault()).Where(i => i != null).ToList();
							data.AddIf(() => true, () => ElImages(_store.ElGetThumbnails(list)));
						}
						break;
					case ElFinderCommand.Duplicate:
						if (targets.HasItems())
						{
							_store.ElGetList(targets.First(), ElFinderListMode.Info, out current);
							var list = targets.Select(i => _store.ElGetList(i).FirstOrDefault()).Where(i => i != null);
							data.AddIf(() => true, () => ElAdded(_store.ElCopy(list, current, false)));
						}
						break;
					default:
						throw new NotImplementedException();
				}

			}
			catch (NotImplementedException)
			{
				data.AddIf(() => true, () => ElError("errUnknownCmd"));
			}
			catch (Exception e)
			{
				data.AddIf(() => true, () => ElError(e.Message));
			}

			return data;
		}

		public IDictionary<String, Object> ProcessPostCommand(ElFinderCommand cmd, IList<TI> target, String content)
		{
			var data = new Dictionary<String, Object>();
			try
			{
				ElFinderWorkingDirectory<TI> current;
				if (target.HasItems())

					switch (cmd)
					{
						case ElFinderCommand.Put:
							{
								var file = _store.ElGetList(target.First(), ElFinderListMode.Children, out current).OfType<ElFinderFile<TI>>().FirstOrDefault(f => f.Id.Equals(target));
								if (file != null && current.Id.Equals(file.IdParent))
								{
									using (var stream = new MemoryStream())
									{
										stream.Write(Encoding.Default.GetBytes(content), 0, Encoding.Default.GetByteCount(content));
										stream.Seek(0, SeekOrigin.Begin);
										data.AddIf(() => true, () => ElChanged(new[] { _store.ElSetContent(file, stream) }));
									}
								}
							}
							break;
						case ElFinderCommand.Upload:
							{
								var ids = new List<ElFinderFile<TI>>();
								var count = HttpContext.Current.Request.Files.Count;
								_store.ElGetList(target.First(), ElFinderListMode.Children, out current);
								if (current != null)
								{
									for (var i = 0; i < count; i++)
									{
										var file = HttpContext.Current.Request.Files[i];
										if (String.IsNullOrEmpty(file.FileName)) continue;
										var id = _store.ElMakeFile(current, file.FileName);
										ids.Add(_store.ElSetContent(id, file.InputStream));
									}
									data
										.AddIf(() => true, () => ElAdded(ids));
								}
							}
							break;
						default:
							throw new NotImplementedException();
					}
			}
			catch (NotImplementedException)
			{
				data.AddIf(() => true, () => ElError("errUnknownCmd"));
			}
			return data;
		}

		~ElFinderController()
		{
			var disposable = _store as IDisposable;
			if (disposable != null)
				disposable.Dispose();
		}
	}
}
