using System.Net;
using System.Reflection;
using HttpServer.Framework;
using HttpServer.Framework.Decorator;
using HttpServer.Listener;
using HttpServer.Router;
using static HttpServer.Listener.ServerListenerContext;

namespace HttpServer
{
	public class Server
	{
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

		public IEnumerable<Type> GetControllers()
		{
			return GetAssembly()
				.GetTypes()
				.Where(t =>
					t.IsSubclassOf(typeof(Controller))
					&& t.GetCustomAttribute<ControllableAttribute>() != null
				);
		}

		public async Task LoadControllers()
		{
			var controllerTypes = GetAssembly()
				.GetTypes()
				.Where(t =>
					t.IsSubclassOf(typeof(Controller))
					&& t.GetCustomAttribute<ControllableAttribute>() != null
				);

			foreach (var type in controllerTypes)
			{
				var controllerInstance = Activator.CreateInstance(type) as Controller;

				var methodsToInvoke = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

				foreach (var method in methodsToInvoke)
				{
					var routeAttribute = method.GetCustomAttribute<RouteAttribute>();
					if (routeAttribute != null)
					{
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

#if DEBUG
	class Program
	{
		static void Main(string[] args)
		{
			Server server = Server.Instance;
			server.Configure();
			server.StartServer();

			Console.WriteLine("Serveur démarré. Appuyez sur 'Enter' pour quitter.");
			Console.ReadLine(); // Garde le serveur en cours d'exécution jusqu'à ce que 'Enter' soit pressé

			server.StopServer();
		}
	}
#endif
}
