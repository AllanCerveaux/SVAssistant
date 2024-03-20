namespace HttpServer.Router
{
	public delegate Task AsyncRouteAction();

	public interface IRoute
	{
		string Path { get; }

		HttpMethod Method { get; }
		Task Execute();
	}

	public class Route : IRoute
	{
		public string Path { get; }

		public HttpMethod Method { get; }

		private readonly AsyncRouteAction _action;

		public Route(string path, HttpMethod method, AsyncRouteAction action)
		{
			Path = path;
			Method = method;
			_action = action;
		}

		public async Task Execute()
		{
			await _action();
		}
	}
}
