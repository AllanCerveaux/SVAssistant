namespace SVAssistant.Decorator
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class RouteAttribute : Attribute
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
