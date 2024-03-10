using System.Net;
using System.Text;
using System.Text.Json;

namespace SVAssistant.Rest
{
	public delegate void HttpRouteAction(
		HttpListenerRequest request,
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

		public void HandleResquest(HttpListenerContext context)
		{
			var request = context.Request;
			var response = context.Response;
			HeaderConfiguration(response);

			var method = request.HttpMethod;
			var path = request.Url?.AbsolutePath;

			RouteHttpResponse routeHttpResponse = new RouteHttpResponse(response);

			var route = routes.FirstOrDefault(route =>
				route.Path == path && route.Method.ToString() == method
			);

			if (route == null)
			{
				routeHttpResponse.Error(HttpStatusCode.NotFound, "Route Not Found");
				return;
			}

			if (isRateLimitExeeded(request.RemoteEndPoint.Address.ToString()))
			{
				routeHttpResponse.Error(HttpStatusCode.TooManyRequests, "Too Many Resquests");
				return;
			}

			if (route.RequireAuthentication && !isTokenValid(request))
			{
				routeHttpResponse.Error(HttpStatusCode.Unauthorized, "Token Invalid or Expire");
				return;
			}

			route.Action(request, routeHttpResponse, context);

			response.Close();
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
				_rateLimitingDic[ipAddr] = new RequestInfo { Count = 1, LastRest = DateTime.Now };
				return false;
			}

			var info = _rateLimitingDic[ipAddr];
			if ((DateTime.Now - info.LastRest) > ResetPeriod)
			{
				info.Count = 1;
				info.LastRest = DateTime.Now;
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
		public readonly HttpListenerResponse _response;

		public RouteHttpResponse(HttpListenerResponse response)
		{
			_response = response;
		}

		public void Json(object data, int stausCode = (int)HttpStatusCode.Accepted)
		{
			_response.ContentType = "application/json";
			_response.StatusCode = stausCode;
			var jsonResponse = JsonSerializer.Serialize(data);
			var buffer = Encoding.UTF8.GetBytes(jsonResponse);
			_response.ContentLength64 = buffer.Length;
			_response.OutputStream.Write(buffer, 0, buffer.Length);	
		}

		public void Error(HttpStatusCode code, string message)
		{
			_response.ContentType = "application/json";
			_response.StatusCode = (int)code;
			var jsonResponse = JsonSerializer.Serialize(new { code, message });
			var buffer = Encoding.UTF8.GetBytes(jsonResponse);
			_response.ContentLength64 = buffer.Length;
			_response.OutputStream.Write(buffer, 0, buffer.Length);
		}
	}

	public class RequestInfo
	{
		public int Count { get; set; }
		public DateTime LastRest { get; set; }
	}
}
