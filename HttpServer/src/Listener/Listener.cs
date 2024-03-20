using System.Net;
using System.Text;
using System.Text.Json;
using HttpServer.Router;

namespace HttpServer.Listener
{
	public class ServerListener
	{
		private static readonly Lazy<ServerListener> _instance = new Lazy<ServerListener>(
			new ServerListener(Routes.Instance)
		);
		public static ServerListener Instance => _instance.Value;

		public string ServerUrl { get; }
		private readonly ServerListenerConfig _config;

		private readonly HttpListener _listener;

		private readonly IRoutes _routes;

		private volatile bool _keepGoing = false;
		private static Task _mainLoopTask;

		private ServerListener(IRoutes routes)
		{
			_config = new ServerListenerConfig();
			Console.WriteLine($"ServerListener: {_config.ServerUrl}");
			ServerUrl = _config.ServerUrl;
			_listener = new HttpListener { Prefixes = { $"{_config.ServerUrl}" } };

			_routes = routes;
		}

		public void Run()
		{
			if (_mainLoopTask != null && !_mainLoopTask.IsCompleted)
				return;
			Console.WriteLine($"Server start at port ${ServerUrl}");
			_keepGoing = true;
			_listener.Start();
			_mainLoopTask = MainLoopAsync();
		}

		public void Stop()
		{
			_keepGoing = false;
			_listener.Stop();
			_listener.Close();
			_mainLoopTask?.Wait();
		}

		private async Task MainLoopAsync()
		{
			while (_keepGoing)
			{
				try
				{
					var context = await _listener.GetContextAsync();
					lock (_listener)
					{
						if (_keepGoing)
						{
							ServerListenerContext.CurrentResponse = context.Response;
							ServerListenerContext.CurrentRequest = context.Request;
							Task.Run(() => ProcessRequestAsync(context));
						}
					}
				}
				catch (Exception e)
					when (e is HttpListenerException || e is ObjectDisposedException)
				{
					break;
				}
				catch (Exception e)
				{
					Console.WriteLine($"Unexpected error in server loop: {e.Message}");
				}
			}
		}

		private async Task ProcessRequestAsync(HttpListenerContext context)
		{
			using (var response = context.Response)
			{
				try
				{
					await _routes.Handler(context);
				}
				catch (UnauthorizedAccessException e)
				{
					Console.WriteLine($"Authorization error: {e.Message}");

					response.StatusCode = (int)HttpStatusCode.Unauthorized;
					response.ContentType = "application/json";
					var errorResponse = JsonSerializer.Serialize(
						new { code = response.StatusCode, error = "Unauthorized" }
					);
					var buffer = Encoding.UTF8.GetBytes(errorResponse);
					response.ContentLength64 = buffer.Length;
					await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
				}
				finally
				{
					response.OutputStream.Close();
				}
			}
		}
	}
}
