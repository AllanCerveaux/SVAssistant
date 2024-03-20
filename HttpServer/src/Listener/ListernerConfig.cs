namespace HttpServer.Listener
{
	public class ServerListenerConfig
	{
		public string Host { get; set; } = "localhost";
		public int Port { get; set; } = 8000;
		public string ServerUrl => $"http://{Host}:{Port}/";
	}
}
