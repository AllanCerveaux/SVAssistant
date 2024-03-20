using System.Security.Cryptography;

namespace HttpServer.Security
{
	public class Encryption
	{
		/// <summary>
		/// Gets the salt size for the password hashing process.
		/// </summary>
		private static int size =>
			int.Parse(Environment.GetEnvironmentVariable("PASSWORD_SALT_SIZE") ?? "16");

		/// <summary>
		/// Gets the number of iterations for the PBKDF2 password hashing process.
		/// </summary>
		private static int itteration =>
			int.Parse(Environment.GetEnvironmentVariable("PBKDF2_ITERATIONS") ?? "10000");

		/// <summary>
		/// Gets the hash size for the output of the password hashing process.
		/// </summary>
		private static int hashSize =>
			int.Parse(Environment.GetEnvironmentVariable("HASH_SIZE") ?? "20");

		/// <summary>
		/// Hashes a password using PBKDF2 with a secure randomly generated salt.
		/// </summary>
		/// <param name="password">The password to hash.</param>
		/// <returns>A Base64 encoded string containing both the salt and the hashed password.</returns>
		public static string Hash(string password)
		{
			byte[] salt = new byte[size];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(salt);
			}

			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, itteration))
			{
				byte[] hash = pbkdf2.GetBytes(hashSize);

				byte[] hashBytes = new byte[size + hashSize];
				Array.Copy(salt, 0, hashBytes, 0, size);
				Array.Copy(hash, 0, hashBytes, size, hashSize);

				return Convert.ToBase64String(hashBytes);
			}
		}

		/// <summary>
		/// Verifies a password against a stored hash using PBKDF2.
		/// </summary>
		/// <param name="password">The password to verify.</param>
		/// <param name="currentHash">The stored hash to compare against, which includes both the salt and the password hash.</param>
		/// <returns>True if the password matches the stored hash; otherwise, false.</returns>
		public static bool VerifyPassword(string password, string curretHash)
		{
			byte[] hashBytes = Convert.FromBase64String(curretHash);
			byte[] salt = new byte[size];
			Array.Copy(hashBytes, 0, salt, 0, size);

			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, itteration))
			{
				byte[] hash = pbkdf2.GetBytes(hashSize);

				for (int i = 0; i < hashSize; i++)
				{
					if (hashBytes[i + size] != hash[i])
					{
						return false;
					}
				}

				return true;
			}
		}
	}
}
