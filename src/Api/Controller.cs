using SVAssistant.Http.Routes;

namespace SVAssistant.Api
{
	public abstract class Controller
	{
		public readonly IHttpResponseService Response;
		public readonly IHttpRequestService Request;
		public readonly IRouteHandler Route;

		protected Controller(
			IHttpResponseService response,
			IHttpRequestService request,
			IRouteHandler route
		)
		{
			Response = response;
			Request = request;
			Route = route;
		}

		protected async Task Json(object data)
		{
			try
			{
				await Response.Json(data);
			}
			catch (Exception e)
			{
				ModEntry.Logger.Log($"{e.Message}", StardewModdingAPI.LogLevel.Error);
			}
		}
	}
}
