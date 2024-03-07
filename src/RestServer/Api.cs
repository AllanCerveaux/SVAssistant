using System.Net;
using System.Text;
using Newtonsoft.Json;
using StardewValley;

namespace Rest
{
	internal static class Server
	{
		private const int Port = 8888;
		private const string Host = "localhost";
		private static readonly HttpListener Listener = new HttpListener
		{
			Prefixes = { $"http://{Host}:{Port}/" }
		};

		private static volatile bool _keepGoing = true;

		private static Task _mainLoop;

		public static readonly Routes routes = new Routes();

		public static string ServerUrl()
		{
			return $"http://{Host}:{Port}";
		}

		public static void StartHttpServer()
		{
			if (_mainLoop != null && !_mainLoop.IsCompleted)
				return;
			_mainLoop = MainLoop();
		}

		public static void StopHttpServer()
		{
			_keepGoing = false;
			lock (Listener)
			{
				Listener.Stop();
			}
			try
			{
				_mainLoop.Wait();
			}
			catch { }
		}

		private static async Task MainLoop()
		{
			Listener.Start();
			while (_keepGoing)
			{
				try
				{
					var context = await Listener.GetContextAsync();
					lock (Listener)
					{
						if (_keepGoing)
							ProcessRequest(context);
					}
				}
				catch (Exception e)
				{
					if (e is HttpListenerException)
						return;
				}
			}
		}

		private static void ProcessRequest(HttpListenerContext context)
		{
			using (var response = context.Response)
			{
				try
				{
					routes.HandleResquest(context);
				}
				catch (Exception e)
				{
					response.StatusCode = (int)HttpStatusCode.InternalServerError;
					response.ContentType = "application/json";
					var buffer = Encoding.UTF8.GetBytes(
						JsonConvert.SerializeObject(new { error = "Bad request" })
					);
					response.ContentLength64 = buffer.Length;
					response.OutputStream.Write(buffer, 0, buffer.Length);

					Console.Write($"Process Request Error: {e.Message}");
				}
			}
		}
	}
}
