namespace BlockiFinAid.Helpers;

public static class UAMHelper
{
    public static string PasswordGenerator()
    {
        const int passwordLength = 8;
        // Define all possible characters for the password
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()";
        var random = new Random();

        // Generate the password by selecting random characters from the 'chars' string
        // The password will be 8 characters long
        var password = new string(Enumerable.Repeat(chars, passwordLength)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        return password;

    }
}