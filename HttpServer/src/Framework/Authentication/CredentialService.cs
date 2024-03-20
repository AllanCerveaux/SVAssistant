using HttpServer.Security;

namespace HttpServer.Framework
{
	public class CredentialService
	{
		public static string CurrentPassword { get; private set; }

		public static string GenerateCredential()
		{
			var generator = new Password(8).Generate();

			CurrentPassword = Encryption.Hash(generator);

			return generator;
		}
	}
}
