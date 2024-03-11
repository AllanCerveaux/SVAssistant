using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

using SVAssistant;

public static class JsonWebToken
{
	/// <summary>
	/// Gets the secret key used for signing JWT tokens.
	/// </summary>
	private static string secret =>
		Environment.GetEnvironmentVariable("JWT_SECRET") ?? "007b1fae-8cf4-482b-bfb4-d26a7906d5b0";

	/// <summary>
	/// Signs and generates a JWT token with specified claims.
	/// </summary>
	/// <param name="sub">The subject claim identifying the principal that is the subject of the JWT.</param>
	/// <param name="uniqueName">A unique name claim that provides a unique identifier for the user.</param>
	/// <param name="audience">The audience claim identifying the recipients that the JWT is intended for.</param>
	/// <returns>A signed JWT token as a string.</returns>
	public static string Sign(string sub, string uniqueName, string audiance, string admin)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
		var credential = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, sub),
			new Claim(JwtRegisteredClaimNames.UniqueName, uniqueName),
			new Claim(JwtRegisteredClaimNames.Aud, audiance),
			new Claim("admin", admin),
			new Claim(JwtRegisteredClaimNames.Exp, DateTime.Now.AddDays(7).ToString()),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		var token = new JwtSecurityToken(
			claims: claims,
			expires: DateTime.Now.AddDays(7),
			signingCredentials: credential
		);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
	
	/// <summary>
	/// Verifies the validity of a JWT token and returns the corresponding ClaimsPrincipal.
	/// </summary>
	/// <param name="token">The JWT token to be verified.</param>
	/// <returns>A ClaimsPrincipal derived from the token if it is valid.</returns>
	/// <remarks>
	/// @TODO: Review activation of <c>ValidateIssuer</c> and <c>ValidateAudience</c>.
	/// </remarks>
	public static ClaimsPrincipal Verify(string token)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
		var tokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = key,
			ValidateIssuer = false,
			ValidateAudience = false,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero
		};

		var tokenHandler = new JwtSecurityTokenHandler();

		SecurityToken validatedToken;
		var principal = tokenHandler.ValidateToken(
			token,
			tokenValidationParameters,
			out validatedToken
		);

		ModEntry.Logger.Log($"{validatedToken.ToString()}", StardewModdingAPI.LogLevel.Info);
		
		return principal;
	}

	public static string GetJwtTokenFromHeader(System.Net.HttpListenerRequest request)
	{
		var token = request.Headers["Authorization"]?.Split(' ').LastOrDefault();
		if (string.IsNullOrEmpty(token))
		{
			return null;
		}

		return token;
	}
}
