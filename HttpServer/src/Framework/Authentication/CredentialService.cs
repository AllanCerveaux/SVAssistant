using HttpServer.Security;

namespace HttpServer.Framework
{
	public class CredentialService
	{
		private static AsyncLocal<string> currentPassword = new AsyncLocal<string>();
		public static string CurrentPassword
		{
			get => currentPassword.Value;
			set => currentPassword.Value = value;
		}

		public static string GenerateCredential()
		{
			var generator = new Password(8).Generate();

			CurrentPassword = Encryption.Hash(generator);

			return generator;
		}
	}
}
