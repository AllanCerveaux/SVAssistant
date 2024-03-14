using System.Net;
using StardewValley;
using SVAssistant.Decorator;
using SVAssistant.Http.Routes;

namespace SVAssistant.Api
{
	public class SignInDTO
	{
		public string password { get; set; }
	}

	public class TokenDTO
	{
		public string token { get; set; }
	}

	public static class Authentication
	{
		public static Dictionary<string, string> GenerateCredential()
		{
			var generator = new PasswordGenerator.Password(8).Generate();
			string password = Encryption.Hash(generator);

			return new Dictionary<string, string>
			{
				{ "raw", generator },
				{ "password", password }
			};
		}
	}

	public class AuthenticationController : Controller
	{
		public AuthenticationController(
			IHttpResponseService response,
			IHttpRequestService request,
			IRouteHandler route
		)
			: base(response, request, route) { }

		[Post("/connect")]
		public async Task SignIn()
		{
			var signInBodyData = await Request.ReadAsyncJsonBody<SignInDTO>();

			var isValid = Encryption.VerifyPassword(
				signInBodyData.password,
				ModEntry._cache["password"]
			);

			try
			{
				if (!isValid)
				{
					await Response.Error(
						"Cannot find instance of game!",
						HttpStatusCode.BadRequest
					);
					return;
				}

				string token = JsonWebToken.Sign(
					Game1.player.UniqueMultiplayerID.ToString(),
					Game1.player.Name.ToString(),
					Game1.GetSaveGameName().ToString(),
					Game1.player.IsMainPlayer.ToString()
				);

				await Json(new TokenDTO { token = token });
				return;
			}
			catch (Exception e)
			{
				ModEntry.Logger.Log(
					$"Internal Error: {e.Message}",
					StardewModdingAPI.LogLevel.Error
				);
			}
		}
	}
}
