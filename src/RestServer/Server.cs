using System.Net;
using System.Text;
using System.Text.Json;
using StardewModdingAPI;

namespace SVAssistant.Rest
{
	public class HttpServerConfig
	{
		public string Host { get; set; } = "localhost";
		public int Port { get; set; } = 8000;

		public string ServerUrl => $"http://{Host}:{Port}/";
	}

	public class HttpServer
	{
		private static HttpServer _instance;
		private static readonly object _lock = new object();

		public static HttpServer Instance
		{
			get
			{
				lock (_lock)
				{
					if (_instance == null)
					{
						_instance = new HttpServer();
					}
					return _instance;
				}
			}
		}

		private readonly HttpServerConfig _config;
		public string ServerUrl { get; }
		private readonly HttpListener _listener;
		private volatile bool _keepGoing = false;
		private static Task _mainLoopTask;
		public static Routes routes { get; } = Routes.Instance;

		private HttpServer()
		{
			_config = new HttpServerConfig();
			ModEntry.Logger.Log($"{_config.ServerUrl}", LogLevel.Info);
			_listener = new HttpListener { Prefixes = { $"{_config.ServerUrl}" } };
			ServerUrl = _config.ServerUrl;
		}

		public void Start()
		{
			if (_mainLoopTask != null && !_mainLoopTask.IsCompleted)
				return;

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
							Task.Run(() => ProcessRequestAsync(context));
					}
				}
				catch (Exception e)
					when (e is HttpListenerException || e is ObjectDisposedException)
				{
					break;
				}
				catch (Exception e)
				{
					ModEntry.Logger.Log(
						$"Unexpected error in server loop: {e.Message}",
						LogLevel.Error
					);
				}
			}
		}

		/// <remarks>@TODO: fine better way to catch routes exeption</remarks>
		private static async Task ProcessRequestAsync(HttpListenerContext context)
		{
			using (var response = context.Response)
			{
				try
				{
					await routes.HandleRequestAsync(context);
				}
				catch (UnauthorizedAccessException e)
				{
					ModEntry.Logger.Log($"Authorization error: {e.Message}", LogLevel.Error);

					response.StatusCode = (int)HttpStatusCode.Unauthorized;
					response.ContentType = "application/json";
					var errorResponse = JsonSerializer.Serialize(
						new { code = response.StatusCode, error = "Unauthorized" }
					);
					var buffer = Encoding.UTF8.GetBytes(errorResponse);
					response.ContentLength64 = buffer.Length;
					await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
				}
				catch (Exception e)
				{
					ModEntry.Logger.Log($"Process Request Error: {e.Message}", LogLevel.Error);

					response.StatusCode = (int)HttpStatusCode.InternalServerError;
					response.ContentType = "application/json";
					var errorResponse = JsonSerializer.Serialize(
						new { code = response.StatusCode, error = "Internal server error" }
					); // Message d'erreur modifié pour refléter l'erreur interne
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
