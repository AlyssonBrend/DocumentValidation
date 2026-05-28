using DocumentValidator.Application.Interfaces;
using DocumentValidator.Domain.Entities;

namespace DocumentValidator.Application.Validators.European;

// Validates ICAO TD1 format European ID cards (3 lines × 30 characters)
public class EuropeanIdValidator : IDocumentValidator
{
    private static readonly int[] Weights = [7, 3, 1];

    public DocumentValidationResult Validate(string value)
    {
        var result = new DocumentValidationResult();

        var mrz = value.Replace("\n", "").Replace("\r", "").Replace(" ", "").ToUpper();

        if (mrz.Length < 90)
        {
            result.Errors.Add("European ID MRZ must be 90 characters (3 lines of 30).");
            return result;
        }

        // Line 1: positions 0–29
        // Line 2: positions 30–59  — Document number: 30–38, check digit: 39
        // Line 3: positions 60–89

        string documentNumber = mrz.Substring(30, 9);
        char checkChar = mrz[39];

        if (!char.IsDigit(checkChar))
        {
            result.Errors.Add("Invalid MRZ format: check digit is not a digit.");
            return result;
        }

        int calculated = CalculateChecksum(documentNumber);
        int expected = checkChar - '0';

        if (calculated != expected)
        {
            result.Errors.Add($"Invalid document number checksum. Expected {expected}, got {calculated}.");
            return result;
        }

        // Date of birth: line 2 positions 13–18 (mrz 43–48), check at 49
        if (mrz.Length >= 50)
        {
            string dob = mrz.Substring(43, 6);
            if (char.IsDigit(mrz[49]) && CalculateChecksum(dob) != (mrz[49] - '0'))
                result.Warnings.Add("Date of birth checksum mismatch.");
        }

        // Expiry: line 2 positions 20–25 (mrz 50–55), check at 56
        if (mrz.Length >= 57)
        {
            string expiry = mrz.Substring(50, 6);
            if (char.IsDigit(mrz[56]) && CalculateChecksum(expiry) != (mrz[56] - '0'))
                result.Warnings.Add("Expiry date checksum mismatch.");
        }

        result.IsValid = result.Errors.Count == 0;
        result.ConfidenceScore = result.Warnings.Count == 0 ? 95 : 75;
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
