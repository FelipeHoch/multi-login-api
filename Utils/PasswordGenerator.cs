namespace multi_login.Utils;

public class PasswordGenerator
{
    private const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";
    private const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string NumericChars = "0123456789";
    private const string SpecialChars = "!@#$%^&*()_-+=[]{}|:<>?";

    public static string GeneratePassword(int length, bool includeLowercase = true, bool includeUppercase = true,
        bool includeNumeric = true, bool includeSpecial = true)
    {
        if (length <= 0)
            throw new ArgumentException("Password length must be greater than zero.");

        string charSet = "";
        if (includeLowercase)
            charSet += LowercaseChars;
        if (includeUppercase)
            charSet += UppercaseChars;
        if (includeNumeric)
            charSet += NumericChars;
        if (includeSpecial)
            charSet += SpecialChars;

        if (string.IsNullOrEmpty(charSet))
            throw new ArgumentException("At least one character set must be included in the password generation.");

        Random random = new Random();
        char[] password = new char[length];

        for (int i = 0; i < length; i++)
        {
            password[i] = charSet[random.Next(0, charSet.Length)];
        }

        return new string(password);
    }
}