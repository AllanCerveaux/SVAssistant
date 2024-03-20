using System.IdentityModel.Tokens.Jwt;
using static HttpServer.Listener.ServerListenerContext;

namespace HttpServer.JWT
{
	public class JwtHelper
	{
		public static JwtPayload GetPayloadFromJwt()
		{
			var token = ServiceListenerRequestService.HeaderAuthorization;
			var tokenHandler = new JwtSecurityTokenHandler();
			var jwtSecurityToken = tokenHandler.ReadJwtToken(token);

			return jwtSecurityToken.Payload;
		}
	}
}
