using Microsoft.Extensions.DependencyInjection;
using StardewValley;
using SVAssistant.Decorator;
using SVAssistant.Http.Routes;
using SVAssistant.Utils;

namespace SVAssistant.Api
{
	public class FarmerDTO
	{
		public string name { get; set; }
		public int money { get; set; }
		public JobsDTO jobs { get; set; }
		public List<AchievementDTO> achievements { get; set; }
	}

	public static class FarmerEntity
	{
		public static FarmerDTO GetFarmerDTO(long id)
		{
			StardewValley.Farmer farmer = Game1.getFarmer(id);
			return new FarmerDTO
			{
				name = farmer.Name,
				money = farmer.Money,
				jobs = Jobs.getPlayerJobs(farmer),
				achievements = Achievements.GetPlayerAchievement(farmer),
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
			// @TODO: create attribute to pass current user in request
			var jwtHelper = ModEntry.ServiceProvider.GetService<IJwtHelper>();
			var user = jwtHelper.GetPayloadFromJwt();
			await Json(FarmerEntity.GetFarmerDTO(long.Parse(user.Sub)));
		}
	}
}
