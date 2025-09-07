using BlockiFinAid.Data.Models;

namespace BlockiFinAid.Helpers;

public static class UAMHelper
{
    public record BankAccountRecord(string BankName, string BranchCode, int accountNumberLength);

    private static List<BankAccountRecord> bankAccountRecords = new List<BankAccountRecord>()
    {
        new BankAccountRecord("Standard Bank", "051001", 9),
        new BankAccountRecord("Capitec Bank", "470010", 9),
        new BankAccountRecord("First National Bank", "250655", 9),
        new BankAccountRecord("ABSA", "632005", 9),
        new BankAccountRecord("Nedbank", "198765", 9),

    };
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

    public static string GetAminEmail() => "admin@blockifinaid.com";
    public static BankAccountModel GenerateRandomBankAccount()
    {

        Random random = new Random();
        int randomIndex = random.Next(0, bankAccountRecords.Count);
        
        var accountProvider = bankAccountRecords[randomIndex];

        var randomAccountNumber = random.Next(100000000);
        if (accountProvider.BankName.Length != 0)
        {
            var account = new BankAccountModel
            {
                BankName = accountProvider.BankName,
                BankBranchCode = accountProvider.BranchCode,
                BankAccountNumber = randomAccountNumber.ToString(),
            };

            return account;
        }

        return null;
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
    public static string GeneratePaymentTransactionNumber()
    {
        // Create a new random number generator
        Random random = new Random();

        // to ensure it has at most 7 digits.
        int randomNumber = random.Next(1000000000);

        // Format the number with leading zeros to ensure it's always 7 digits long
        string numberPart = randomNumber.ToString("D9");

        // Prepend 'S' to the generated number
        string studentNumber = $"TRX{numberPart}";

        return studentNumber;
    }
    public static string GeneratePaymentGroupNumber()
    {
        // Create a new random number generator
        Random random = new Random();

        // to ensure it has at most 7 digits.
        int randomNumber = random.Next(100000);

        // Format the number with leading zeros to ensure it's always 7 digits long
        string numberPart = randomNumber.ToString("D5");

        // Prepend 'S' to the generated number
        string studentNumber = $"GRP{numberPart}";

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