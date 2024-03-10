using System.Net;
using Rest;

namespace SVAssistant.Api
{
	public class FarmerDTO
	{
		public string Name { get; set; }
		public int Money { get; set; }
		public JobsDTO Jobs { get; set; }
		public List<AchievementDTO> Achievements { get; set; }
	}

	internal static class FarmerEntity
	{
		public static FarmerDTO GetFarmerDTO(StardewValley.Farmer farmer)
		{
			return new FarmerDTO
			{
				Name = farmer.Name,
				Money = farmer.Money,
				Jobs = Jobs.getPlayerJobs(farmer),
				Achievements = Achievements.GetPlayerAchievement(farmer),
			};
		}
	}

	internal class FarmerController
	{
		public static HttpRouteAction HandleGetFarmer(StardewValley.Farmer farmer)
		{
			return (
				HttpListenerRequest request,
				RouteHttpResponse response,
				HttpListenerContext? context,
				bool RequireAuthentication
			) => response.Json(FarmerEntity.GetFarmerDTO(farmer));
		}
	}
}
