using DocumentValidator.Application.Interfaces;
using DocumentValidator.Domain.Entities;

namespace DocumentValidator.Application.Validators.Passport;

public class PassportMrzValidator : IDocumentValidator
{
    private static readonly int[] Weights = [7, 3, 1];

    public DocumentValidationResult Validate(string value)
    {
        var result = new DocumentValidationResult();

        var mrz = value.Replace("\n", "").Replace("\r", "").Replace(" ", "").ToUpper();

        // TD3 format: 2 lines of 44 chars = 88 total
        if (mrz.Length < 88)
        {
            result.Errors.Add("MRZ must be 88 characters (2 lines of 44).");
            return result;
        }

        // Passport number is chars 44–52 (line 2, positions 0–8), check digit at 53
        string passportNumber = mrz.Substring(44, 9);
        char checkDigit = mrz[53];
        int calculated = CalculateChecksum(passportNumber);

        if (calculated.ToString() != checkDigit.ToString())
        {
            result.Errors.Add($"Invalid MRZ passport number checksum. Expected {calculated}, got {checkDigit}.");
            return result;
        }

        // Date of birth: positions 57–62, check at 63
        string dob = mrz.Substring(57, 6);
        if (CalculateChecksum(dob) != (mrz[63] - '0'))
            result.Warnings.Add("Date of birth checksum mismatch.");

        // Expiry: positions 65–70, check at 71
        string expiry = mrz.Substring(65, 6);
        if (CalculateChecksum(expiry) != (mrz[71] - '0'))
            result.Warnings.Add("Expiry date checksum mismatch.");

        result.IsValid = result.Errors.Count == 0;
        result.ConfidenceScore = result.Warnings.Count == 0 ? 95 : 70;
        return result;
    }

    private int CalculateChecksum(string input)
    {
        int total = 0;
        for (int i = 0; i < input.Length; i++)
            total += GetCharValue(input[i]) * Weights[i % 3];
        return total % 10;
    }

    private static int GetCharValue(char c)
    {
        if (char.IsDigit(c)) return c - '0';
        if (c == '<') return 0;
        if (char.IsLetter(c)) return c - 'A' + 10;
        return 0;
    }
}
