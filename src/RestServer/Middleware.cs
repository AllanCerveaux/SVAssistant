using System.Net;
using System.Security.Claims;

namespace SVAssistant.Rest.Middleware
{
	public static class JWTMiddleware
	{
		public static ClaimsPrincipal VerifyJWT(HttpListenerRequest request, out bool isValid)
		{
			try
			{
				var token = JsonWebToken.GetJwtTokenFromHeader(request);
				var principal = JsonWebToken.Verify(token);
				if (principal != null)
				{
					isValid = true;
					return principal;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine($"JWT Exeception: {e.Message}");
				throw new UnauthorizedAccessException("Invalid or missing JWT");
			}

			isValid = false;
			return null;
		}
	}
}
