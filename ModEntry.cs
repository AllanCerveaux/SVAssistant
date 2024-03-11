using System.Diagnostics;
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

		public override void Entry(IModHelper helper)
		{
			this.Config = this.LoadConfig();
			Logger = this.Monitor;

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
				Process.Start(new ProcessStartInfo(Server.ServerUrl()) { UseShellExecute = true });
			}
			catch (Exception e)
			{
				Console.WriteLine($"Une erreur est survenue : {e.Message}");
			}
		}

		public void OnSaveLoaded(object? sender, SaveLoadedEventArgs eventArgs)
		{
			Server.StartHttpServer();
			this.LoadApiRoutes();

			HUDMessage hUDMessage = new HUDMessage(
				$"SVAssitant has running on {Server.ServerUrl()}",
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
			Server.StopHttpServer();
		}

		public void OnInputChanged(object? sender, ButtonsChangedEventArgs eventArgs)
		{
			if (Config.Controls.toogleMenu.JustPressed())
			{
				this.OpenInBrowser(Server.ServerUrl());
			}
		}

		public void LoadApiRoutes()
		{
			Server.routes.Post("/signin", AuthenticationController.SignIn);
		}
	}
}
