using System.IdentityModel.Tokens.Jwt;
using System.Net;
using StardewValley;
using SVAssistant.Decorator;
using SVAssistant.Rest;

namespace SVAssistant.Api
{
	public class FarmerDTO
	{
		public string Name { get; set; }
		public int Money { get; set; }
		public JobsDTO Jobs { get; set; }
		public List<AchievementDTO> Achievements { get; set; }
	}

	public static class FarmerEntity
	{
		public static FarmerDTO GetFarmerDTO(long id)
		{
			StardewValley.Farmer farmer = Game1.getFarmer(id);

			return new FarmerDTO
			{
				Name = farmer.Name,
				Money = farmer.Money,
				Jobs = Jobs.getPlayerJobs(farmer),
				Achievements = Achievements.GetPlayerAchievement(farmer),
			};
		}
	}

	public class FamerController
	{
		public RouteAction GetCurrentFarmer(StardewValley.Farmer farmer)
		{
			return [JWT]
			(RouteHttpRequest request, RouteHttpResponse response, HttpListenerContext? context) =>
			{
				var token = HttpServer.Routes.header.SecurityToken as JwtSecurityToken;
				ModEntry.Logger.Log($"{token.Subject}", StardewModdingAPI.LogLevel.Info);
				return response.Json(FarmerEntity.GetFarmerDTO(long.Parse(token.Subject)));
			};
		}
	}
}
