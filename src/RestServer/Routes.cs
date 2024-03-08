using System.Net;
using System.Text;

namespace Rest
{
	public delegate void HttpRouteAction(
		HttpListenerRequest request,
		RouteHttpResponse response,
		HttpListenerContext? context
	);

	internal class Routes
	{
		private readonly List<Route> routes = new List<Route>();

		private void Add(Route route)
		{
			routes.Add(route);
		}

		public void Get(string path, HttpRouteAction action)
		{
			Add(new Route(path, action, HttpMethod.Get));
		}

		public void Post(string path, HttpRouteAction action)
		{
			Add(new Route(path, action, HttpMethod.Post));
		}

		public void HandleResquest(HttpListenerContext context)
		{
			var request = context.Request;
			var response = new RouteHttpResponse(context.Response);
			var path = request.Url.AbsolutePath;
			var method = request.HttpMethod;

			var route = routes.FirstOrDefault(route =>
				route.Path == path && route.Method.ToString() == method
			);

			if (route != null)
			{
				route.Action(request, response, context);
				return;
			}

			context.Response.StatusCode = (int)HttpStatusCode.NotFound;
			context.Response.Close();
		}
	}

	internal class Route
	{
		public string Path { get; }
		public HttpMethod Method { get; }
		public HttpRouteAction Action { get; }

		public Route(string path, HttpRouteAction action, HttpMethod method)
		{
			this.Path = path;
			this.Action = action;
			this.Method = method;
		}
	}

	public class RouteHttpResponse
	{
		private readonly HttpListenerResponse _response;

		public RouteHttpResponse(HttpListenerResponse response)
		{
			_response = response;
		}

		public void Json(object data, int stausCode = (int)HttpStatusCode.Accepted)
		{
			_response.ContentType = "application/json";
			_response.StatusCode = stausCode;
			var jsonResponse =  System.Text.Json.JsonSerializer.Serialize(data);
			var buffer = Encoding.UTF8.GetBytes(jsonResponse);
			_response.ContentLength64 = buffer.Length;
			_response.OutputStream.Write(buffer, 0, buffer.Length);
			_response.OutputStream.Close();
		}
	}
}
