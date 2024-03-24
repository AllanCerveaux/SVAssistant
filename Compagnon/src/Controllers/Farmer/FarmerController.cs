using System.IdentityModel.Tokens.Jwt;
using HttpServer.Framework;
using HttpServer.Framework.Decorator;

namespace Compagnon.Controllers
{
	[Controllable]
	public class FarmerController : Controller
	{
		[JWT]
		[Get("/me")]
		public async Task GetCurrentFarmer([CurrentUser] JwtPayload payload)
		{
			var entity = new FarmerEntity(long.Parse(payload.Sub));
			await Json(entity.GetFarmerInformation());
		}
	}
}
