using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PasswordGenerator
{
	/// <summary>
	/// Defines the settings for password generation.
	/// </summary>
	public interface IPasswordSettings
	{
		bool IncludeLowercase { get; }
		bool IncludeUppercase { get; }
		bool IncludeNumeric { get; }
		bool IncludeSpecial { get; }
		int PasswordLength { get; set; }
		string CharacterSet { get; }
		int MaximumAttempts { get; }
		int MinimumLength { get; }
		int MaximumLength { get; }
		string SpecialCharacters { get; set; }
	}

	/// <summary>
	/// Implements password settings with configurable options for including different character types and defining length constraints.
	/// </summary>
	public class Settings : IPasswordSettings
	{
		private const string LowercaseCharacters = "abcdefghijklmnopqrstuvwxyz";
		private const string UppercaseCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private const string NumericCharacters = "0123456789";
		private const string DefaultSpecialCharacters = @"!#$%&*";
		private const int DefaultMinPasswordLength = 4;
		private const int DefaultMaxPasswordLength = 256;
		public string SpecialCharacters { get; set; }

		/// <summary>
		/// Initializes a new instance of the Settings class with custom password generation preferences.
		/// </summary>
		/// <param name="includeLowercase">Specifies whether lowercase letters should be included in the password character set.</param>
		/// <param name="includeUppercase">Specifies whether uppercase letters should be included in the password character set.</param>
		/// <param name="includeNumeric">Specifies whether numeric digits should be included in the password character set.</param>
		/// <param name="includeSpecial">Specifies whether special characters should be included in the password character set.</param>
		/// <param name="passwordLength">Specifies the desired length of the generated passwords.</param>
		/// <param name="maximumAttempts">Specifies the maximum number of attempts to generate a valid password.</param>
		public Settings(
			bool includeLowercase,
			bool includeUppercase,
			bool includeNumeric,
			bool includeSpecial,
			int passwordLength,
			int maximumAttempts
		)
		{
			IncludeLowercase = includeLowercase;
			IncludeUppercase = includeUppercase;
			IncludeNumeric = includeNumeric;
			IncludeSpecial = includeSpecial;
			PasswordLength = passwordLength;
			MaximumAttempts = maximumAttempts;
			MinimumLength = DefaultMinPasswordLength;
			MaximumLength = DefaultMaxPasswordLength;
			SpecialCharacters = DefaultSpecialCharacters;
			CharacterSet = ComposePasswordCharacterSet(
				includeLowercase,
				includeUppercase,
				includeNumeric,
				includeSpecial
			);
		}

		public bool IncludeLowercase { get; private set; }
		public bool IncludeUppercase { get; private set; }
		public bool IncludeNumeric { get; private set; }
		public bool IncludeSpecial { get; private set; }
		public int PasswordLength { get; set; }
		public string CharacterSet { get; private set; }
		public int MaximumAttempts { get; }
		public int MinimumLength { get; }
		public int MaximumLength { get; }

		/// <summary>
		/// Builds the character set used for password generation based on the specified inclusion flags.
		/// </summary>
		/// <param name="includeLowercase">Include lowercase letters if true.</param>
		/// <param name="includeUppercase">Include uppercase letters if true.</param>
		/// <param name="includeNumeric">Include numeric digits if true.</param>
		/// <param name="includeSpecial">Include special characters if true.</param>
		/// <returns>A string representing the character set for password generation.</returns>
		private string ComposePasswordCharacterSet(
			bool includeLowercase,
			bool includeUppercase,
			bool includeNumeric,
			bool includeSpecial
		)
		{
			var characterSet = new StringBuilder();
			if (includeLowercase)
				characterSet.Append(LowercaseCharacters);

			if (includeUppercase)
				characterSet.Append(UppercaseCharacters);

			if (includeNumeric)
				characterSet.Append(NumericCharacters);

			if (includeSpecial)
				characterSet.Append(SpecialCharacters);
			return characterSet.ToString();
		}
	}

	public class Password
	{
		private const int DefaultPasswordLength = 8;
		private const int DefaultMaxPasswordAttempts = 10000;
		private const bool DefaultIncludeNumeric = true;
		private const bool DefaultIncludeLowercase = true;
		private const bool DefaultIncludeUppercase = true;
		private const bool DefaultIncludeSpecial = true;

		/// <summary>
		/// The random number generator used for password generation.
		/// </summary>
		private static RandomNumberGenerator _rng;

		/// <summary>
		/// Initializes a new instance of the Password class with customizable settings for password generation.
		/// </summary>
		/// <param name="passwordLength">The desired length of the passwords to be generated. Defaults to a pre-defined length.</param>
		/// <param name="includeNumeric">Indicates whether numeric characters should be included in the password. Defaults to true.</param>
		/// <param name="includeLowercase">Indicates whether lowercase alphabetical characters should be included in the password. Defaults to true.</param>
		/// <param name="includeUppercase">Indicates whether uppercase alphabetical characters should be included in the password. Defaults to true.</param>
		/// <param name="includeSpecial">Indicates whether special characters should be included in the password. Defaults to true.</param>
		/// <param name="maximumAttempts">The maximum number of attempts to generate a valid password. This helps prevent infinite loops in cases where password requirements might not be met easily. Defaults to a specified maximum.</param>
		public Password(
			int passwordLength = DefaultPasswordLength,
			bool includeNumeric = DefaultIncludeNumeric,
			bool includeLowercase = DefaultIncludeLowercase,
			bool includeUppercase = DefaultIncludeUppercase,
			bool includeSpecial = DefaultIncludeSpecial,
			int maximumAttempts = DefaultMaxPasswordAttempts
		)
		{
			Settings = new Settings(
				includeLowercase,
				includeUppercase,
				includeNumeric,
				includeSpecial,
				passwordLength,
				maximumAttempts
			);
			_rng = RandomNumberGenerator.Create();
		}

		public IPasswordSettings Settings { get; set; }

		/// <summary>
		/// Generates a new password based on the configured settings.
		/// </summary>
		/// <returns>A generated password string that conforms to the specified settings, or an error message if the password could not be generated within the constraints.</returns>
		public string Generate()
		{
			string password;
			if (
				!LengthIsValid(
					Settings.PasswordLength,
					Settings.MinimumLength,
					Settings.MaximumLength
				)
			)
			{
				password =
					$"Password length invalid. Must be between {Settings.MinimumLength} and {Settings.MaximumLength} characters long";
			}
			else
			{
				var passwordAttempts = 0;
				do
				{
					password = GenerateRandomPassword(Settings);
				} while (
					passwordAttempts < Settings.MaximumAttempts && !IsValid(Settings, password)
				);
				password = IsValid(Settings, password) ? password : "Password not Valid";
			}

			return password;
		}

		/// <summary>
		/// Generates a password string that adheres to the specified password settings.
		/// </summary>
		/// <param name="settings">The settings to use for password generation.</param>
		/// <returns>A randomly generated password string.</returns>
		private static string GenerateRandomPassword(IPasswordSettings settings)
		{
			const int maximumIdenticalConsecutiveChars = 2;
			var password = new char[settings.PasswordLength];

			var characters = settings.CharacterSet.ToCharArray();
			var shuffledChars = Shuffle(characters.Select(x => x)).ToArray();

			var shuffledCharacterSet = string.Join(null, shuffledChars);
			var characterSetLength = shuffledCharacterSet.Length;

			for (
				var characterPosition = 0;
				characterPosition < settings.PasswordLength;
				characterPosition++
			)
			{
				password[characterPosition] = shuffledCharacterSet[
					GetRandomNumberInRange(0, characterSetLength - 1)
				];

				var moreThanTwoIdenticalInARow =
					characterPosition > maximumIdenticalConsecutiveChars
					&& password[characterPosition] == password[characterPosition - 1]
					&& password[characterPosition - 1] == password[characterPosition - 2];

				if (moreThanTwoIdenticalInARow)
					characterPosition--;
			}

			return string.Join(null, password);
		}

		/// <summary>
		/// Generates a random integer within the specified range.
		/// </summary>
		/// <param name="min">The inclusive lower bound of the range.</param>
		/// <param name="max">The exclusive upper bound of the range.</param>
		/// <returns>A random integer between <paramref name="min"/> and <paramref name="max"/>.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="min"/> is greater than <paramref name="max"/>.</exception>
		private static int GetRandomNumberInRange(int min, int max)
		{
			if (min > max)
				throw new ArgumentOutOfRangeException(
					nameof(min),
					"min must be less than or equal to max."
				);

			var data = new byte[sizeof(int)];
			_rng.GetBytes(data);
			var randomNumber = BitConverter.ToInt32(data, 0);

			return (int)Math.Floor((double)(min + Math.Abs(randomNumber % (max - min))));
		}

		/// <summary>
		/// Validates a password against the specified password settings.
		/// </summary>
		/// <param name="settings">The password settings to validate against.</param>
		/// <param name="password">The password to validate.</param>
		/// <returns>True if the password meets all specified settings; otherwise, false.</returns>
		private static bool IsValid(IPasswordSettings settings, string password)
		{
			const string regexLowercase = @"[a-z]";
			const string regexUppercase = @"[A-Z]";
			const string regexNumeric = @"[\d]";

			var lowerCaseIsValid =
				!settings.IncludeLowercase
				|| settings.IncludeLowercase && Regex.IsMatch(password, regexLowercase);
			var upperCaseIsValid =
				!settings.IncludeUppercase
				|| settings.IncludeUppercase && Regex.IsMatch(password, regexUppercase);
			var numericIsValid =
				!settings.IncludeNumeric
				|| settings.IncludeNumeric && Regex.IsMatch(password, regexNumeric);

			var specialIsValid = !settings.IncludeSpecial;

			if (settings.IncludeSpecial && !string.IsNullOrWhiteSpace(settings.SpecialCharacters))
			{
				var listA = settings.SpecialCharacters.ToCharArray();
				var listB = password.ToCharArray();

				specialIsValid = listA.Any(x => listB.Contains(x));
			}

			return lowerCaseIsValid
				&& upperCaseIsValid
				&& numericIsValid
				&& specialIsValid
				&& LengthIsValid(password.Length, settings.MinimumLength, settings.MaximumLength);
		}

		/// <summary>
		/// Shuffles the elements of a given sequence using a random order.
		/// </summary>
		/// <typeparam name="T">The type of elements in the sequence.</typeparam>
		/// <param name="items">The sequence of items to shuffle.</param>
		private static IEnumerable<T> Shuffle<T>(IEnumerable<T> items)
		{
			return from item in items orderby Guid.NewGuid() select item;
		}

		/// <summary>
		/// Validates that a given password length falls within the specified minimum and maximum length constraints.
		/// </summary>
		/// <param name="passwordLength">The length of the password to validate.</param>
		/// <param name="minLength">The minimum acceptable length of the password.</param>
		/// <param name="maxLength">The maximum acceptable length of the password.</param>
		/// <returns>True if the password length is within the specified bounds; otherwise, false.</returns>
		private static bool LengthIsValid(int passwordLength, int minLength, int maxLength)
		{
			return passwordLength >= minLength && passwordLength <= maxLength;
		}
	}
}
