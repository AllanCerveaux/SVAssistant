using System.Net;
using System.Reflection;
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
			this.Monitor.Log("Hello World!", LogLevel.Info);

			_server = Server.Instance;

			helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
			helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
			helper.Events.GameLoop.ReturnedToTitle += this.OnReturnToTitle;
		}

		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
		{
			_server.Configure();
			_server.LoadControllers();

			var credential = CredentialService.GenerateCredential();

			this.Monitor.Log($"{_server.ServerUrl}, password: {credential}", LogLevel.Info);
		}

		private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
		{
			_server.StartServer();
			HUDMessage hUDMessage = new HUDMessage(
				$"SVAssitant has running on {_server.ServerUrl}",
				3
			);
			Game1.addHUDMessage(hUDMessage);
		}

		private void OnReturnToTitle(object? sender, ReturnedToTitleEventArgs e)
		{
			_server.StopServer();
		}

		internal class SignInDTO
		{
			public string password { get; set; }
		}

		internal class TokenDTO
		{
			public string token { get; set; }
		}

		[Controllable]
		public class AuthenticationController : Controller
		{
			[Post("/connect")]
			public async Task SignIn()
			{
				ModEntry.Logger.Log("Connect", LogLevel.Info);
				var signInBodyData = await Request.ReadAsyncJsonBody<SignInDTO>();
				ModEntry.Logger.Log($"signInBodyData {signInBodyData}", LogLevel.Info);
				var isValid = Encryption.VerifyPassword(
					signInBodyData.password,
					CredentialService.CurrentPassword
				);
				ModEntry.Logger.Log($"isValid {isValid}", LogLevel.Info);

				if (!isValid)
				{
					await Response.Error(
						"Cannot find instance of game!",
						HttpStatusCode.BadRequest
					);
					return;
				}

				try
				{
					string token = JsonWebToken.Sign(
						Guid.NewGuid().ToString(),
						"michel",
						"La ferme Drucker",
						"true"
					);

					await Json(new TokenDTO { token = token });
				}
				catch (Exception e)
				{
					Console.WriteLine($"Internal Error: {e.Message}");
				}
			}
		}
	}
}
