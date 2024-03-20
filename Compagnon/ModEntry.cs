using System.Net;
using HttpServer;
using HttpServer.Framework;
using HttpServer.Framework.Decorator;
using HttpServer.JWT;
using HttpServer.Security;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace Compagnon
{
	public class ModEntry : Mod
	{
		private Server _server;
		public static IMonitor Logger;

		public override void Entry(IModHelper helper)
		{
			Logger = this.Monitor;

			_server = Server.Instance;

			helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
			helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
			helper.Events.GameLoop.ReturnedToTitle += this.OnReturnToTitle;
		}

		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
		{
			_server.Configure();
			_server.LoadControllers();
			_server.StartServer();
			var credential = CredentialService.GenerateCredential();
			Logger.Log($"Password: {credential}", LogLevel.Info);
		}

		private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
		{
			HUDMessage hUDMessage = new HUDMessage(
				$"SVAssitant has running on {_server.ServerUrl}",
				3
			);
			Game1.addHUDMessage(hUDMessage);

			// Game1.chatBox.addInfoMessage($"Password to connect on app: {credential}");
		}

		private void OnReturnToTitle(object? sender, ReturnedToTitleEventArgs e)
		{
			_server.StopServer();
		}
	}
}
