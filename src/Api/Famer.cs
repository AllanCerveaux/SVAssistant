using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
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
		[JWT]
		public async Task<Task> GetCurrentFarmer(
			RouteHttpRequest request,
			RouteHttpResponse response,
			HttpListenerContext? context
		)
		{
			var principal = Routes.Instance.header.ClaimPrincipal;
			if (principal == null)
			{
				ModEntry.Logger.Log("SecurityToken is not set.", StardewModdingAPI.LogLevel.Info);
				return response.Error("Unauthorized - Token not set.", HttpStatusCode.Unauthorized);
			}

			var subjectClaim =
				principal.FindFirst(ClaimTypes.NameIdentifier)
				?? principal.FindFirst(JwtRegisteredClaimNames.Sub);
			if (subjectClaim == null)
			{
				ModEntry.Logger.Log("Subject claim is missing.", StardewModdingAPI.LogLevel.Info);
				return response.Error(
					"Unauthorized - JWT Mal Formatted",
					HttpStatusCode.Unauthorized
				);
			}

			return response.Json(FarmerEntity.GetFarmerDTO(long.Parse(subjectClaim.Value)));
		}
	}
}
