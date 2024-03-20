using System.Net;
using System.Text;

namespace HttpServer.Router
{
	public interface IRoutes
	{
		Task Handler(HttpListenerContext context);
		void RegisterRoute(IRoute route);
		void RegisterRoutes(IRoute[] routes);
	}

	public class Routes : IRoutes
	{
		private static readonly Lazy<IRoutes> _instance = new Lazy<IRoutes>(() => new Routes());
		public static IRoutes Instance => _instance.Value;
		private readonly List<IRoute> _routes = new List<IRoute>();

		private Routes() { }

		public async Task Handler(HttpListenerContext context)
		{
			var requestPath = context.Request.Url?.AbsolutePath;
			var method = new HttpMethod(context.Request.HttpMethod);

			var route = _routes.FirstOrDefault(r =>
				r.Path.Equals(requestPath, StringComparison.OrdinalIgnoreCase) && r.Method == method
			);

			if (route != null)
			{
				await route.Execute();
			}
			else
			{
				// @TODO: Change to Service Response Error;
				context.Response.StatusCode = 404;
				var buffer = Encoding.UTF8.GetBytes("Not Found");
				context.Response.OutputStream.Write(buffer, 0, buffer.Length);
				context.Response.Close();
			}
		}

		public void RegisterRoute(IRoute route)
		{
			_routes.Add(route);
		}

		public void RegisterRoutes(IRoute[] routes)
		{
			foreach (var route in routes)
			{
				_routes.Add(route);
			}
		}
	}
}
