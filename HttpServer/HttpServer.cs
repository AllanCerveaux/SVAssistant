using System.Net;
using System.Reflection;
using HttpServer.Framework;
using HttpServer.Framework.Decorator;
using HttpServer.JWT;
using HttpServer.Listener;
using HttpServer.Router;
using Microsoft.IdentityModel.Tokens;
using static HttpServer.Listener.ServerListenerContext;

namespace HttpServer
{
	public class Server
	{
		private static AsyncLocal<string> currentPassword = new AsyncLocal<string>();
		public static string CurrentPassword
		{
			get => currentPassword.Value;
			set => currentPassword.Value = value;
		}
		private static readonly Lazy<Server> _instance = new Lazy<Server>(() => new Server());
		public static Server Instance => _instance.Value;
		private ServerListener? _listener;
		private IRoutes? _routes;
		public string? ServerUrl;

		private Server() { }

		public void Configure()
		{
			_routes = Routes.Instance;
			_listener = ServerListener.Instance;
			ServerUrl = _listener.ServerUrl;
		}

		public void StartServer()
		{
			if (_listener == null)
			{
				Console.WriteLine("Server not configure, start with minimal configuration");
				Configure();
			}

			_listener.Run();

			_routes.RegisterRoutes(
				new IRoute[]
				{
					new Route(
						"/health",
						HttpMethod.Get,
						() => new ServiceListenerResponseService().Json(new { status = "UP" })
					),
				}
			);
		}

		public void StopServer()
		{
			_listener.Stop();
		}

		public Assembly GetAssembly()
		{
			var assembly = AppDomain
				.CurrentDomain.GetAssemblies()
				.FirstOrDefault(a => a.GetName().Name == "Compagnon");

			if (assembly != null)
			{
				return assembly;
			}

			return Assembly.GetExecutingAssembly();
		}

		public void LoadControllers()
		{
			var controllerTypes = GetAssembly()
				.GetTypes()
				.Where(t =>
					t.IsSubclassOf(typeof(Controller))
					&& t.GetCustomAttribute<ControllableAttribute>() != null
				);
			Console.WriteLine($"Controller count {controllerTypes.Count()}");

			foreach (var type in controllerTypes)
			{
				Console.WriteLine($"Controller {type}");
				var controllerInstance = Activator.CreateInstance(type) as Controller;
				Console.WriteLine($"Controller instance {controllerInstance.GetType()}");

				var methodsToInvoke = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

				foreach (var method in methodsToInvoke)
				{
					var routeAttribute = method.GetCustomAttribute<RouteAttribute>();
					if (routeAttribute != null)
					{
						Console.WriteLine($"Route {routeAttribute.GetType()} - {method.Name}");
						AsyncRouteAction action = () =>
							(Task)method.Invoke(controllerInstance, null);
						Routes.Instance.RegisterRoute(
							new Route(routeAttribute.Path, routeAttribute.Method, action)
						);
					}
				}
			}
		}
	}
}
