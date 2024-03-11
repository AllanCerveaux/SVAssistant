using System.Net;
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

	public class FamerController
	{
		public static RouteAction GetCurrentFarmer(StardewValley.Farmer farmer)
		{
			return (
				RouteHttpRequest request,
				RouteHttpResponse response,
				HttpListenerContext? context,
				bool RequireAuthentication
			) => response.Json(FarmerEntity.GetFarmerDTO(farmer));
		}
	}
}
