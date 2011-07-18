using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ElFinder.Integration
{
	public abstract class ElFinderMvcController<TI, TSI> : Controller where TSI : IElFinderStoreInterface<TI>, new()
	{
		private readonly ElFinderController<TI, TSI> _controller;

		protected ElFinderMvcController()
		{
			_controller = new ElFinderController<TI, TSI>();
		}

		public ActionResult ElFinder(ElFinderCommand cmd, int? init, int? tree, IList<TI> target, String name, IList<TI> src, IList<TI> dst, IList<TI> targets, int? cut, int? download, String type, Int32? width, Int32? height)
		{
			return new JsonNetResult { Data = _controller.ProcessCommand(cmd, init, tree, target, name, src, dst, targets, cut, download, type, width, height) };
		}

		[HttpPost]
		public ActionResult ElFinder(ElFinderCommand cmd, IList<TI> target, String content)
		{
			return new JsonNetResult { Data = _controller.ProcessPostCommand(cmd, target, content) };
		}
	}
}
