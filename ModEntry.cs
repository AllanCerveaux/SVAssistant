using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SVAssistant.Api;
using SVAssistant.Decorator;
using SVAssistant.Framework;
using SVAssistant.Http.Routes;
using SVAssistant.Utils;

namespace SVAssistant
{
	internal sealed class ModEntry : Mod
	{
		public static readonly Dictionary<string, string> _cache = new Dictionary<string, string>();
		private ModConfig Config = null;
		public static IMonitor Logger;
		private HttpServer _httpServer;

		private static readonly AsyncLocal<ServiceProvider> _serviceProvider =
			new AsyncLocal<ServiceProvider>();
		public static ServiceProvider ServiceProvider
		{
			get => _serviceProvider.Value;
			set => _serviceProvider.Value = value;
		}

		public override void Entry(IModHelper helper)
		{
			Logger = this.Monitor;
			ServiceProvider = DependencyInjectionConfig.ConfigureServices();

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
			this.Config = this.LoadConfig();

			_httpServer = ServiceProvider.GetRequiredService<HttpServer>();
			this.LoadApiRoutes();
		}

		public void OnSaveLoaded(object? sender, SaveLoadedEventArgs eventArgs)
		{
			_httpServer.Start();
			HUDMessage hUDMessage = new HUDMessage(
				$"SVAssitant has running on {_httpServer.ServerUrl}",
				3
			);
			Game1.addHUDMessage(hUDMessage);

			var credential = Authentication.GenerateCredential();
			Game1.chatBox.addInfoMessage($"To connect API you can use this password {credential}");
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
			var routes = ServiceProvider.GetRequiredService<IRouteHandler>();
			var controllers = ServiceProvider.GetServices<Controller>();
			foreach (var controller in controllers)
			{
				var methods = controller
					.GetType()
					.GetMethods()
					.Where(m => m.GetCustomAttributes<RouteAttribute>().Any());
				foreach (var method in methods)
				{
					var attribute = method.GetCustomAttribute<RouteAttribute>();
					Logger.Log(
						$"RegistedRoute for {controller.GetType()} - {method.Name} on {attribute.Path} - {attribute.Method}"
					);
					routes.RegisterRoute(
						new Route(
							attribute.Path,
							attribute.Method,
							async (context) =>
							{
								await (Task)method.Invoke(controller, null);
							}
						)
					);
				}
			}
		}
	}
}
