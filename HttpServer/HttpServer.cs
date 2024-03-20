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
					var jwtAttribute = method.GetCustomAttribute<JWTAttribute>();
					if (routeAttribute != null)
					{
						Console.WriteLine($"Route {routeAttribute.GetType()} - {method.Name}");
						AsyncRouteAction action = () =>
						{
							if (jwtAttribute != null)
							{
								Console.WriteLine($"JWT {jwtAttribute.GetType()} - {method.Name}");
								jwtAttribute.IsAuthorized();
							}
							var parameters = method.GetParameters();
							var args = new object[parameters.Length];
							for (int i = 0; i < parameters.Length; i++)
							{
								var parameter = parameters[i];
								var currentUserAttribute =
									parameter.GetCustomAttributes<CurrentUserAttribute>();

								if (currentUserAttribute != null)
								{
									args[i] = JwtHelper.GetPayloadFromJwt();
								}
								else
								{
									args[i] = GetDefaultValue(parameter.ParameterType);
								}
							}

							return (Task)method.Invoke(controllerInstance, args);
						};
						Routes.Instance.RegisterRoute(
							new Route(routeAttribute.Path, routeAttribute.Method, action)
						);
					}
				}
			}
		}

		public object GetDefaultValue(Type t)
		{
			return t.IsValueType ? Activator.CreateInstance(t) : null;
		}
	}
}
