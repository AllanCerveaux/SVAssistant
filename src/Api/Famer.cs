using Microsoft.Extensions.DependencyInjection;
using StardewValley;
using SVAssistant.Decorator;
using SVAssistant.Http.Routes;

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

	public class FamerController : Controller
	{
		public FamerController(
			IHttpResponseService response,
			IHttpRequestService request,
			IRouteHandler route
		)
			: base(response, request, route) { }

		[JWT]
		[Get("/get-current-farmer")]
		public async Task GetCurrentFarmer()
		{
			var jwtHelper = ModEntry.ServiceProvider.GetService<IJwtHelper>();
			var user = jwtHelper.GetPayloadFromJwt();
			ModEntry.Logger.Log($"Farmer Controller: {user}", StardewModdingAPI.LogLevel.Info);
			await Json(FarmerEntity.GetFarmerDTO(long.Parse(user.Sub)));
		}
	}
}
