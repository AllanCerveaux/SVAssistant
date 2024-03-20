using System.Net;
using HttpServer.Framework;
using HttpServer.Framework.Decorator;
using HttpServer.JWT;
using HttpServer.Security;
using StardewValley;

namespace Compagnon.Controllers
{
	internal class SignInDTO
	{
		public string password { get; set; }
	}

	internal class TokenDTO
	{
		public string token { get; set; }
	}

	[Controllable]
	public class AuthenticationController : Controller
	{
		[Post("/connect")]
		public async Task SignIn()
		{
			ModEntry.Logger.Log("Connection");
			var signInBodyData = await Request.ReadAsyncJsonBody<SignInDTO>();

			var isValid = Encryption.VerifyPassword(
				signInBodyData.password,
				CredentialService.CurrentPassword
			);

			if (!isValid)
			{
				await Response.Error("Cannot find instance of game!", HttpStatusCode.BadRequest);
				return;
			}

			try
			{
				string token = JsonWebToken.Sign(
					Game1.player.UniqueMultiplayerID.ToString(),
					Game1.player.Name.ToString(),
					Game1.GetSaveGameName().ToString(),
					Game1.player.IsMainPlayer.ToString()
				);

				await Json(new TokenDTO { token = token });
			}
			catch (Exception e)
			{
				Console.WriteLine($"Internal Error: {e.Message}");
			}
		}
	}
}
