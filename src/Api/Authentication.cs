using System.Net;
using System.Text;
using System.Text.Json;

using StardewValley;
using SVAssistant.Rest;

namespace SVAssistant.Api
{
	public class SignInDTO
	{
		public string password { get; set; }
	}

	public static class Authentication
	{
		public static Dictionary<string, string> GenerateCredential()
		{
			// var generator = new PasswordGenerator.Password(8).Generate();
			var generator = "password";
			string password = Encryption.Hash(generator);

			return new Dictionary<string, string>
			{
				{ "raw", generator },
				{ "password", password }
			};
		}

		public static void SigninRequest() { }
	}

	public static class AuthenticationController
	{
		public static void SignIn(
			HttpListenerRequest request,
			RouteHttpResponse response,
			HttpListenerContext? context,
			bool RequireAuthentication = false
		)
		{
			string requestBody =  new StreamReader(request.InputStream, request.ContentEncoding).ReadToEnd();
			var signInData = JsonSerializer.Deserialize<SignInDTO>(requestBody);

			var isValid = Encryption.VerifyPassword(
				signInData.password,
				ModEntry._cache["password"]
			);

            try {
                if(!isValid)
                {
                    response.Error(HttpStatusCode.BadRequest, "Cannot find instance of game!");
                    return;
                }				
				string token = JsonWebToken.Sign(Game1.player.UniqueMultiplayerID.ToString(), Game1.player.Name.ToString(), Game1.GetSaveGameName().ToString(), Game1.player.IsMainPlayer.ToString());
                response.Json(new {token = $"Bearer {token}"});
				return;
            }
            catch(Exception e)
            {
                ModEntry.Logger.Log($"Something wrong when generate JWT: {e.Message}", StardewModdingAPI.LogLevel.Error);
            }
		}
	}
}
