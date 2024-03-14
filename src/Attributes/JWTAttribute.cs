using System.Reflection;
using MethodDecorator.Fody.Interfaces;
using Microsoft.IdentityModel.Tokens;
using SVAssistant.Api;
using SVAssistant.Http.Routes;

namespace SVAssistant.Decorator
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	public class JWTAttribute : Attribute, IMethodDecorator
	{
		private IHttpRequestService _httpRequest;

		public void Init(object instance, MethodBase method, object[] args)
		{
			if (instance is Controller controller)
			{
				_httpRequest = controller.Request;
			}
		}

		public void OnEntry()
		{
			var _jwtToken =
				_httpRequest.HeaderAuthorization?.Replace("Bearer ", "")
				?? throw new UnauthorizedAccessException("JWT Token is missing.");
			try
			{
				var principal = JsonWebToken.Verify(_jwtToken);
			}
			catch (SecurityTokenValidationException e) // Catchez l'exception spécifique et non Exception générale
			{
				ModEntry.Logger.Log($"JWT OnEntry: {e}", StardewModdingAPI.LogLevel.Error);
				throw new UnauthorizedAccessException("Invalid JWT", e);
			}
		}

		public void OnException(Exception exception)
		{
			ModEntry.Logger.Log(
				$"JWT OnException: {exception.Message}",
				StardewModdingAPI.LogLevel.Error
			);
		}

		public void OnExit() { }
	}
}
