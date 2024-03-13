using System.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SVAssistant.Api;
using SVAssistant.Framework;
using SVAssistant.Rest;

namespace SVAssistant
{
	internal sealed class ModEntry : Mod
	{
		public static readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
		public static IMonitor Logger;
		private ModConfig Config = null;
		private HttpServer _httpServer;

		public override void Entry(IModHelper helper)
		{
			Logger = this.Monitor;

			helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
			helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
			helper.Events.GameLoop.ReturnedToTitle += this.OnReturnToTitle;
			helper.Events.Input.ButtonsChanged += this.OnInputChanged;
		}

		private ModConfig LoadConfig()
		{
			return this.Helper.ReadConfig<ModConfig>();
		}

		private void OpenInBrowser(string url)
		{
			try
			{
				Process.Start(
					new ProcessStartInfo(_httpServer.ServerUrl) { UseShellExecute = true }
				);
			}
			catch (Exception e)
			{
				Logger.Log($"Une erreur est survenue : {e.Message}", LogLevel.Error);
			}
		}

		private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
		{
			_httpServer = HttpServer.Instance;
			this.Config = this.LoadConfig();
		}

		public void OnSaveLoaded(object? sender, SaveLoadedEventArgs eventArgs)
		{
			_httpServer.Start();
			this.LoadApiRoutes();

			HUDMessage hUDMessage = new HUDMessage(
				$"SVAssitant has running on {_httpServer.ServerUrl}",
				3
			);
			Game1.addHUDMessage(hUDMessage);

			var credential = Authentication.GenerateCredential();
			Game1.chatBox.addInfoMessage(
				$"To connect API you can use this password {credential["raw"]}"
			);

			_cache.Add("password", credential["password"]);
		}

		public void OnReturnToTitle(object? sender, ReturnedToTitleEventArgs eventArgs)
		{
			_httpServer.Stop();
			_cache.Clear();
		}

		public void OnInputChanged(object? sender, ButtonsChangedEventArgs eventArgs)
		{
			if (Config.Controls.toogleMenu.JustPressed())
			{
				this.OpenInBrowser(_httpServer.ServerUrl);
			}
		}

		public void LoadApiRoutes()
		{
			FamerController famerController = new FamerController();
			HttpServer.routes.Post("/signin", AuthenticationController.SignIn);
			HttpServer.routes.Get("/current-farmer", famerController.GetCurrentFarmer);
		}
	}
}
