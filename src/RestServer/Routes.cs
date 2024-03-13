using System.Collections.Concurrent;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SVAssistant.Rest
{
	public delegate Task RouteAction(
		RouteHttpRequest request,
		RouteHttpResponse response,
		HttpListenerContext? context
	);

	public class Routes
	{
		private static readonly Lazy<Routes> _instance = new Lazy<Routes>(() => new Routes());
		public static Routes Instance => _instance.Value;

		private readonly List<Route> routes = new List<Route>();
		public RoutesHeader header { get; set; }

		private Routes() { }

		private void Add(Route route)
		{
			routes.Add(route);
		}

		public void Get(string path, RouteAction action)
		{
			Add(new Route(path, action, HttpMethod.Get));
		}

		public void Post(string path, RouteAction action)
		{
			Add(new Route(path, action, HttpMethod.Post));
		}

		public async Task HandleRequestAsync(HttpListenerContext context)
		{
			header = new RoutesHeader(context.Request, context.Response);
			header.Cors();

			var request = context.Request;
			var response = context.Response;
			var method = request.HttpMethod;
			var path = request.Url?.AbsolutePath;

			RouteHttpRequest routeHttpRequest = new RouteHttpRequest(request);
			RouteHttpResponse routeHttpResponse = new RouteHttpResponse(response);

			var route = routes.FirstOrDefault(route =>
				route.Path == path && route.Method.ToString() == method
			);

			if (route == null)
			{
				await routeHttpResponse.Error("Route Not Found", HttpStatusCode.NotFound);
				return;
			}

			if (RouteRateLimit.isRateLimitExeeded(request.RemoteEndPoint.Address.ToString()))
			{
				await routeHttpResponse.Error("Too Many Resquests", HttpStatusCode.TooManyRequests);
				return;
			}

			await route.Action(routeHttpRequest, routeHttpResponse, context);
		}
	}

	public class Route
	{
		public string Path { get; }
		public HttpMethod Method { get; }
		public RouteAction Action { get; }

		public Route(string path, RouteAction action, HttpMethod method)
		{
			this.Path = path;
			this.Action = action;
			this.Method = method;
		}
	}

	public class RouteHttpResponse
	{
		private HttpListenerResponse _response;

		public RouteHttpResponse(HttpListenerResponse response)
		{
			_response = response;
		}

		private async Task ResponseAsync(string data, string contentType, int statusCode)
		{
			_response.ContentType = contentType;
			_response.StatusCode = statusCode;
			var buffer = Encoding.UTF8.GetBytes(data);
			_response.ContentLength64 = buffer.Length;
			using (var stream = _response.OutputStream)
			{
				await stream.WriteAsync(buffer, 0, buffer.Length);
			}
			_response.Close();
		}

		public async Task Json(object data, int statusCode = (int)HttpStatusCode.Accepted)
		{
			var jsonResponse = JsonSerializer.Serialize(data);
			await ResponseAsync(jsonResponse, "application/json", statusCode);
		}

		public async Task Error(
			string message,
			HttpStatusCode statusCode = HttpStatusCode.BadRequest
		)
		{
			await Json(new { code = (int)statusCode, message }, (int)statusCode);
		}
	}

	public class RouteHttpRequest
	{
		private HttpListenerRequest _request;

		public RouteHttpRequest(HttpListenerRequest request)
		{
			_request = request;
		}

		public async Task<string> ReadAsyncJsonBody()
		{
			using (var reader = new StreamReader(_request.InputStream, _request.ContentEncoding))
			{
				return await reader.ReadToEndAsync();
			}
		}
	}

	public class RoutesHeader
	{
		public HttpListenerRequest Request { get; set; }
		public HttpListenerResponse Response { get; set; }
		private AsyncLocal<ClaimsPrincipal> _asyncClaimPrincipal =
			new AsyncLocal<ClaimsPrincipal>();
		public ClaimsPrincipal ClaimPrincipal
		{
			get => _asyncClaimPrincipal.Value;
			set => _asyncClaimPrincipal.Value = value;
		}

		public RoutesHeader(HttpListenerRequest request, HttpListenerResponse response)
		{
			Request = request;
			Response = response;
		}

		public void Cors(string corsOrigin = "*", string corsMethods = "GET, PUT")
		{
			Response.AppendHeader("Access-Control-Allow-Origin", corsOrigin);
			Response.AppendHeader("Access-Control-Allow-Methods", corsMethods);
		}

		public string? GetAuthorization()
		{
			return Request.Headers["Authorization"];
		}
	}

	public class RouteRateLimit
	{
		private static readonly ConcurrentDictionary<string, RequestInfo> _rateLimitingDic =
			new ConcurrentDictionary<string, RequestInfo>();

		// @TODO: Change this arbitary value.
		private const int Limit = 100;
		private static readonly TimeSpan ResetPeriod = TimeSpan.FromHours(1);

		public static bool isRateLimitExeeded(string ipAddr)
		{
			var now = DateTime.UtcNow;
			var info = _rateLimitingDic.GetOrAdd(
				ipAddr,
				_ => new RequestInfo { Count = 1, LastReset = now }
			);

			if ((now - info.LastReset) > ResetPeriod)
			{
				info.Count = 1;
				info.LastReset = now;
				return false;
			}

			if (info.Count >= Limit)
			{
				return true;
			}

			Interlocked.Increment(ref info.Count);
			return false;
		}
	}

	public class RequestInfo
	{
		public int Count;
		public DateTime LastReset { get; set; }
	}
}
