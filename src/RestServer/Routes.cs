using System.Net;
using System.Text;
using System.Text.Json;

namespace SVAssistant.Rest
{
	public delegate Task HttpRouteAction(
		RouteHttpRequest request,
		RouteHttpResponse response,
		HttpListenerContext? context,
		bool RequireAuthentication = false
	);

	internal class Routes
	{
		private readonly List<Route> routes = new List<Route>();

		private void Add(Route route)
		{
			routes.Add(route);
		}

		public void Get(string path, HttpRouteAction action, bool requireAuthentication = false)
		{
			Add(new Route(path, action, HttpMethod.Get, requireAuthentication));
		}

		public void Post(string path, HttpRouteAction action, bool requireAuthentication = false)
		{
			Add(new Route(path, action, HttpMethod.Post, requireAuthentication));
		}

		public async Task HandleResquest(HttpListenerContext context)
		{
			var request = context.Request;
			var response = context.Response;
			HeaderConfiguration(response);

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

			if (isRateLimitExeeded(request.RemoteEndPoint.Address.ToString()))
			{
				await routeHttpResponse.Error("Too Many Resquests", HttpStatusCode.TooManyRequests);
				return;
			}

			if (route.RequireAuthentication && !isTokenValid(request))
			{
				await routeHttpResponse.Error("Token Invalid or Expire", HttpStatusCode.Unauthorized);
				return;
			}

			await route.Action(routeHttpRequest, routeHttpResponse, context);
		}

		private static void HeaderConfiguration(
			HttpListenerResponse response,
			string CorsOrigin = "*",
			string CorsMethods = "GET, POST"
		)
		{
			response.Headers.Add("Access-Control-Allow-Origin", CorsOrigin);
			response.Headers.Add("Access-Control-Allow-Methods", CorsMethods);
		}

		private readonly Dictionary<string, RequestInfo> _rateLimitingDic =
			new Dictionary<string, RequestInfo>();

		// @TODO: Change this arbitary value.
		private const int Limit = 100;
		private readonly TimeSpan ResetPeriod = TimeSpan.FromHours(1);

		private bool isRateLimitExeeded(string ipAddr)
		{
			if (!_rateLimitingDic.ContainsKey(ipAddr))
			{
				_rateLimitingDic[ipAddr] = new RequestInfo { Count = 1, LastReset = DateTime.Now };
				return false;
			}

			var info = _rateLimitingDic[ipAddr];
			if ((DateTime.Now - info.LastReset) > ResetPeriod)
			{
				info.Count = 1;
				info.LastReset = DateTime.Now;
				return false;
			}

			if (info.Count >= Limit)
			{
				return true;
			}

			info.Count++;
			return false;
		}

		private bool isTokenValid(HttpListenerRequest request)
		{
			bool isValid;
			var jwtMiddleware = Middleware.JWTMiddleware.VerifyJWT(request, out isValid);
			return isValid;
		}
	}

	internal class Route
	{
		public string Path { get; }
		public HttpMethod Method { get; }
		public HttpRouteAction Action { get; }
		public bool RequireAuthentication { get; }

		public Route(
			string path,
			HttpRouteAction action,
			HttpMethod method,
			bool requireAuth = false
		)
		{
			this.Path = path;
			this.Action = action;
			this.Method = method;
			this.RequireAuthentication = requireAuth;
		}
	}

	public class RouteHttpResponse
	{
		private static HttpListenerResponse _response;

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
			using(var stream = _response.OutputStream)
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

		public async Task Error(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
		{
			var jsonResponse = JsonSerializer.Serialize(new { code = (int)statusCode, message });
			await ResponseAsync(jsonResponse, "application/json", (int)statusCode);
		}
	}

	public class RouteHttpRequest
	{
		private static HttpListenerRequest _request;

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

	public class RequestInfo
	{
		public int Count { get; set; }
		public DateTime LastReset { get; set; }
	}
}
