using System.Net;
using SVAssistant.Decorator;
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

		protected Task Json(object data)
		{
			return Response.Json(data);
		}
	}
}
