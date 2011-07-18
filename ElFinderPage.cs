using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ElFinder.Integration
{
	public abstract class ElFinderPage<TI, TSI> : Page where TSI : IElFinderStoreInterface<TI>, new()
	{
		private readonly ElFinderController<TI, TSI> _controller;

		protected ElFinderPage()
		{
			_controller = new ElFinderController<TI, TSI>();
		}

		protected override void Render(HtmlTextWriter writer)
		{
			var request = HttpContext.Current.Request;
			if (!String.IsNullOrEmpty(request.Params["cmd"]))
			{
				IDictionary<String, Object> result;
				if (request.HttpMethod.Equals("GET"))
				{
					result = _controller.ProcessCommand(
						(ElFinderCommand)Enum.Parse(typeof(ElFinderCommand), request.Params["cmd"], true),
						Convert.ToInt32(request.Params["init"] ?? "0"),
						Convert.ToInt32(request.Params["tree"] ?? "0"),
						(request.Params["target"] ?? String.Empty).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(i => (TI)Convert.ChangeType(i, typeof(TI))).ToList(),
						(request.Params["name"] ?? String.Empty),
						(request.Params["src"] ?? String.Empty).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(i => (TI)Convert.ChangeType(i, typeof(TI))).ToList(),
						(request.Params["dst"] ?? String.Empty).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(i => (TI)Convert.ChangeType(i, typeof(TI))).ToList(),
						(request.Params["targets"] ?? String.Empty).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(i => (TI)Convert.ChangeType(i, typeof(TI))).ToList(),
						Convert.ToInt32(request.Params["cut"] ?? "0"),
						Convert.ToInt32(request.Params["download"] ?? "0"),
						request.Params["type"] ?? String.Empty,
						Convert.ToInt32(request.Params["width"] ?? "0"),
						Convert.ToInt32(request.Params["height"] ?? "0")
						);
				}
				else if (request.HttpMethod.Equals("POST"))
				{

					result = _controller.ProcessPostCommand(
						(ElFinderCommand)Enum.Parse(typeof(ElFinderCommand), request.Form["cmd"], true),
						(request.Form["target"] ?? String.Empty).Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(i => (TI)Convert.ChangeType(i, typeof(TI))).ToList(),
						request.Form["content"] ?? String.Empty);
				}
				else
					throw new ArgumentException("Unsupported method");
				HttpContext.Current.Response.ContentType = "application/json";

				var serializer = JsonSerializer.Create(new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
				serializer.Serialize(writer, result);
				writer.Flush();
			}


		}
	}
}
