using System.Net;
using System.Security.Claims;

namespace SVAssistant.Rest.Middleware
{
	public static class JWTMiddleware
	{
		public static ClaimsPrincipal VerifyJWT(HttpListenerRequest request, out bool isValid)
		{
			var token = request.Headers["Authorization"]?.Split(' ').LastOrDefault();
			if (string.IsNullOrEmpty(token))
			{
				isValid = false;
				return null;
			}

			try
			{
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
