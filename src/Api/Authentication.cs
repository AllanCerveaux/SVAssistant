using System.Net;
using System.Text.Json;
using StardewValley;
using SVAssistant.Rest;

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

	public static class AuthenticationController
	{
		public static async Task SignIn(
			RouteHttpRequest request,
			RouteHttpResponse response,
			HttpListenerContext? context,
			bool RequireAuthentication = false
		)
		{
			var requestBody = request.ReadAsyncJsonBody();
			var signInData = JsonSerializer.Deserialize<SignInDTO>(await requestBody);

			var isValid = Encryption.VerifyPassword(
				signInData.password,
				ModEntry._cache["password"]
			);

			try
			{
				if (!isValid)
				{
					await response.Error("Cannot find instance of game!");
					return;
				}

				string token = JsonWebToken.Sign(
					Game1.player.UniqueMultiplayerID.ToString(),
					Game1.player.Name.ToString(),
					Game1.GetSaveGameName().ToString(),
					Game1.player.IsMainPlayer.ToString()
				);

				await response.Json(new TokenDTO { token = token });
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
