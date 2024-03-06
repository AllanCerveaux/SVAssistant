using System.Net;
using Rest;
using StardewValley;

namespace SVAssistant.Famer
{
	internal static class FarmerEntity
	{
		private static string Name { get; } = Game1.player.Name;
		private static int Money { get; } = Game1.player.Money;

		public static object GetFamer()
		{
			return new { name = Name, money = Money };
		}
	}

	internal static class FarmerController
	{
		public static void HandleGetFarmer(
			HttpListenerRequest request,
			RouteHttpResponse response,
			HttpListenerContext? context
		)
		{
			response.Json(FarmerEntity.GetFamer());
		}
	}
}
