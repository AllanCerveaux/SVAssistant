// using System.Net;
// using HttpServer.Framework.Decorator;
// using HttpServer.JWT;
// using HttpServer.Security;

// namespace HttpServer.Framework
// {
// 	internal class SignInDTO
// 	{
// 		public string password { get; set; }
// 	}

// 	internal class TokenDTO
// 	{
// 		public string token { get; set; }
// 	}

// 	[Controllable]
// 	public class AuthenticationController : Controller
// 	{
// 		[Post("/connect")]
// 		public async Task SignIn()
// 		{
// 			var signInBodyData = await Request.ReadAsyncJsonBody<SignInDTO>();
// 			var isValid = Encryption.VerifyPassword(
// 				signInBodyData.password,
// 				CredentialService.CurrentPassword
// 			);

// 			if (!isValid)
// 			{
// 				await Response.Error("Cannot find instance of game!", HttpStatusCode.BadRequest);
// 				return;
// 			}

// 			try
// 			{
// 				string token = JsonWebToken.Sign(
// 					Guid.NewGuid().ToString(),
// 					"michel",
// 					"La ferme Drucker",
// 					"true"
// 				);

// 				await Json(new TokenDTO { token = token });
// 			}
// 			catch (Exception e)
// 			{
// 				Console.WriteLine($"Internal Error: {e.Message}");
// 			}
// 		}
// 	}
// }
