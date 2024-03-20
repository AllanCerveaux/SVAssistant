using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SVAssistant.Http.Routes
{
	public interface IRouteHandler
	{
		Task HandleAsync(HttpListenerContext context);
		void RegisterRoute(IRoute route);
	}

	public interface IRoute
	{
		public string Path { get; }
		public HttpMethod Method { get; }
		public Task ExecuteAsync(HttpListenerContext context);
	}

	public interface IHttpResponseService
	{
		public Task Json(object data);
		public Task Error(string message, HttpStatusCode statusCode);
	}

	public interface IHttpRequestService
	{
		public string? HeaderAuthorization { get; }
		public Task<T> ReadAsyncJsonBody<T>();
	}

	public class Route : IRoute
	{
		public string Path { get; }
		public HttpMethod Method { get; }
		private readonly Func<HttpListenerContext, Task> _action;
		private string _path;
		private HttpMethod _method;

		public Route(string path, HttpMethod method, Func<HttpListenerContext, Task> action)
		{
			Path = path;
			Method = method;
			_action = action;
		}

		public Route(string path, HttpMethod method)
		{
			_path = path;
			_method = method;
		}

		public Task ExecuteAsync(HttpListenerContext context) => _action(context);
	}

	public class HttpResponseService : IHttpResponseService
	{
		private async Task ResponseAsync(
			string data,
			string contentType,
			HttpStatusCode statusCode = HttpStatusCode.OK
		)
		{
			var response = RequestContext.CurrentResponse;

			response.ContentType = contentType;
			response.StatusCode = (int)statusCode;
			var buffer = Encoding.UTF8.GetBytes(data);
			response.ContentLength64 = buffer.Length;
			await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
			response.Close();
		}

		public async Task Json(object data)
		{
			var jsonData = JsonSerializer.Serialize(data);
			await ResponseAsync(jsonData, "application/json");
		}

		public async Task Error(string message, HttpStatusCode statusCode)
		{
			var errorData = JsonSerializer.Serialize(new { message, code = statusCode });
			await ResponseAsync(errorData, "application/json", statusCode);
		}
	}

	public class HttpRequestService : IHttpRequestService
	{
		private NameValueCollection Header => RequestContext.CurrentRequest.Headers;
		public string? HeaderAuthorization => Header["Authorization"];

		public async Task<T> ReadAsyncJsonBody<T>()
		{
			using (
				var reader = new StreamReader(
					RequestContext.CurrentRequest.InputStream,
					RequestContext.CurrentRequest.ContentEncoding
				)
			)
			{
				var data = JsonSerializer.Deserialize<T>(await reader.ReadToEndAsync());
				return data;
			}
		}
	}

	public class Routes : IRouteHandler
	{
		private readonly List<IRoute> _routes;

		public Routes()
		{
			_routes = new List<IRoute>();
		}

		public void RegisterRoute(IRoute route)
		{
			_routes.Add(route);
		}

		public async Task HandleAsync(HttpListenerContext context)
		{
			var requestPath = context.Request.Url?.AbsolutePath;
			var method = new HttpMethod(context.Request.HttpMethod);

			var route = _routes.FirstOrDefault(r =>
				r.Path.Equals(requestPath, StringComparison.OrdinalIgnoreCase) && r.Method == method
			);

			if (route != null)
			{
				await route.ExecuteAsync(context);
			}
			else
			{
				context.Response.StatusCode = 404;
				var buffer = Encoding.UTF8.GetBytes("Not Found");
				context.Response.OutputStream.Write(buffer, 0, buffer.Length);
				context.Response.Close();
			}
		}
	}
}
