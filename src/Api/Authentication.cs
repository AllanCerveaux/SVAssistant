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

	public class Authentication
	{
		private static AsyncLocal<string> currentPassword = new AsyncLocal<string>();
		public static string CurrentPassword
		{
			get => currentPassword.Value;
			set => currentPassword.Value = value;
		}

		public static string GenerateCredential()
		{
			var generator = new PasswordGenerator.Password(8).Generate();
			CurrentPassword = Encryption.Hash(generator);

			return generator;
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
				Authentication.CurrentPassword
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
