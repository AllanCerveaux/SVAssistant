using HttpServer.Router;
using static HttpServer.Listener.ServerListenerContext;

namespace HttpServer.Framework
{
	public abstract class Controller
	{
		public readonly ServiceListenerResponseService Response;
		public readonly ServiceListenerRequestService Request;

		protected Controller()
		{
			Response = new ServiceListenerResponseService();
			Request = new ServiceListenerRequestService();
		}

		protected async Task Json(object data)
		{
			try
			{
				await Response.Json(data);
			}
			catch (Exception e)
			{
				Console.WriteLine($"{e.Message}");
			}
		}
	}
}
