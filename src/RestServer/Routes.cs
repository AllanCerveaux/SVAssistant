using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace SVAssistant.Rest
{
	public delegate Task RouteAction(
		RouteHttpRequest request,
		RouteHttpResponse response,
		HttpListenerContext? context
	);

	public class RoutesHeader()
	{
		public HttpListenerRequest Request { get; set; }
		public HttpListenerResponse Response { get; set; }
		private AsyncLocal<JwtSecurityToken> _asyncSecurityToken =
			new AsyncLocal<JwtSecurityToken>();
		public JwtSecurityToken SecurityToken
		{
			get => _asyncSecurityToken.Value;
			set => _asyncSecurityToken.Value = value;
		}

		public void Cors(string CorsOrigin = "*", string CorsMethods = "GET,PUT")
		{
			Request.Headers.Add("Access-Control-Allow-Origin", CorsOrigin);
			Request.Headers.Add("Access-Control-Allow-Methods", CorsMethods);
		}

		public string? GetAuthorization()
		{
			return Request.Headers.Get("Authorization");
		}
	}

	public class Routes
	{
		private static Routes _instance;
		private static readonly object _lock = new object();
		public static Routes Instance
		{
			get
			{
				lock (_lock)
				{
					if (_instance == null)
					{
						_instance = new Routes();
					}
					return _instance;
				}
			}
		}

		public RoutesHeader header { get; set; }
		private readonly List<Route> routes = new List<Route>();

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
			var request = context.Request;
			var response = context.Response;

			header = new RoutesHeader { Request = request, Response = response };

			header.Cors();

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

			await route.Action(routeHttpRequest, routeHttpResponse, context);
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
	}

	internal class Route
	{
		public string Path { get; }
		public HttpMethod Method { get; }
		public RouteAction Action { get; }
		public bool RequireAuthentication { get; }

		public Route(string path, RouteAction action, HttpMethod method, bool requireAuth = false)
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
