using HttpServer.JWT;
using Microsoft.IdentityModel.Tokens;
using static HttpServer.Listener.ServerListenerContext;

namespace HttpServer.Framework.Decorator
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class JWTAttribute : Attribute
	{
		public void IsAuthorized()
		{
			var _jwtToken = ServiceListenerRequestService.HeaderAuthorization?.Replace(
				"Bearer ",
				""
			);
			if (_jwtToken == null)
			{
				throw new UnauthorizedAccessException("JWT Token is missing.");
			}
			try
			{
				var principal = JsonWebToken.Verify(_jwtToken);
			}
			catch (SecurityTokenValidationException e)
			{
				Console.WriteLine($"JWT OnEntry: {e}");
				throw new UnauthorizedAccessException("Invalid JWT", e);
			}
		}
	}
}
