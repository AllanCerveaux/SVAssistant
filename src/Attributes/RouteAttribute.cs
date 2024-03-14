using System.Reflection;
using SVAssistant.Api;
using SVAssistant.Http.Routes;

namespace SVAssistant.Decorator
{
	[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
	public class RouteAttribute : System.Attribute
	{
		public string Path { get; }
		public HttpMethod Method { get; }

		public RouteAttribute(string path, HttpMethod method)
		{
			Path = path;
			Method = method;
		}
	}

	public class GetAttribute : RouteAttribute
	{
		public GetAttribute(string path)
			: base(path, HttpMethod.Get) { }
	}

	public class PostAttribute : RouteAttribute
	{
		public PostAttribute(string path)
			: base(path, HttpMethod.Post) { }
	}
}
