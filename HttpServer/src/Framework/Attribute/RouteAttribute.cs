namespace HttpServer.Framework.Decorator
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class RouteAttribute(string path, HttpMethod httpMethod) : Attribute
	{
		public string Path => path;
		public HttpMethod Method => httpMethod;
	}

	public class GetAttribute(string path) : RouteAttribute(path, HttpMethod.Get) { }

	public class PostAttribute(string path) : RouteAttribute(path, HttpMethod.Post) { }
}
