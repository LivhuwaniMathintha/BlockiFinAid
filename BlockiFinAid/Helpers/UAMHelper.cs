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

    public static string GenerateStudentNumber()
    {
        // Create a new random number generator
        Random random = new Random();

        // Generate a random number between 0 and 9,999,999
        // to ensure it has at most 7 digits.
        int randomNumber = random.Next(10000000);

        // Format the number with leading zeros to ensure it's always 7 digits long
        string numberPart = randomNumber.ToString("D7");

        // Prepend 'S' to the generated number
        string studentNumber = $"S{numberPart}";

        return studentNumber;
    }

    public static string GenerateRandomDegree()
    {
        // A list of common South African degree abbreviations
        string[] degreeTypes = { "BSc", "BCom", "BEng", "BA", "LLB", "BEd" };

        // A list of specializations relevant to the degrees above
        string[] specializations =
        {
            "Computer Sciences", "Accounting", "Mechanical Engineering",
            "Psychology", "Law", "Education (Intermediate Phase)",
            "Electrical Engineering", "Civil Engineering", "Economics",
            "Information Systems", "Biomedical Sciences", "Nursing"
        };

        Random random = new Random();

        // Select a random degree type
        int degreeTypeIndex = random.Next(degreeTypes.Length);
        string degreeType = degreeTypes[degreeTypeIndex];

        // Select a random specialization
        int specializationIndex = random.Next(specializations.Length);
        string specialization = specializations[specializationIndex];

        // Combine them into a full degree string
        // e.g., "BSc Computer Sciences" or "BEng Civil Engineering"
        return $"{degreeType} {specialization}";
    }
}