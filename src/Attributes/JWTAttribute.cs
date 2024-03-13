using System.Reflection;
using MethodDecorator.Fody.Interfaces;
using Microsoft.IdentityModel.Tokens;
using SVAssistant.Rest;

namespace SVAssistant.Decorator
{
	[AttributeUsage(
		AttributeTargets.Method
			| AttributeTargets.Constructor
			| AttributeTargets.Assembly
			| AttributeTargets.Module
	)]
	public class JWTAttribute : Attribute, IMethodDecorator
	{
		private Routes Routes;

		public void Init(object instance, MethodBase method, object[] args)
		{
			Routes = Routes.Instance;
		}

		public void OnEntry()
		{
			var _jwtToken = Routes.header.GetAuthorization()?.Replace("Bearer ", "");
			if (_jwtToken == null)
			{
				throw new UnauthorizedAccessException("JWT Token is missing.");
			}

			try
			{
				var principal = JsonWebToken.Verify(_jwtToken);
				Routes.header.ClaimPrincipal = principal;
			}
			catch (SecurityTokenValidationException e) // Catchez l'exception spécifique et non Exception générale
			{
				ModEntry.Logger.Log($"JWT OnEntry: {e}", StardewModdingAPI.LogLevel.Error);
				throw new UnauthorizedAccessException("Invalid JWT", e);
			}
		}

		public void OnException(Exception exception) { }

		public void OnExit() { }
	}
}
