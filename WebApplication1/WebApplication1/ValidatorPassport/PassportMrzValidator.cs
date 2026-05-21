using DocumentValidator.Application.Interfaces;
using DocumentValidator.Domain.Entities;

namespace DocumentValidator.Application.Validators;

public class PassportMrzValidator : IDocumentValidator
{
    private readonly int[] weights = [7, 3, 1];

    public DocumentValidationResult Validate(string mrz)
    {
        var result = new DocumentValidationResult();

        mrz = mrz.Replace("\n", "")
                 .Replace("\r", "")
                 .Replace(" ", "");

        if (mrz.Length < 44)
        {
            result.IsValid = false;
            result.Errors.Add("MRZ too short.");
            return result;
        }

        string passportNumber = mrz.Substring(44, 9);

        char checkDigit = mrz[53];

        int calculated = CalculateChecksum(passportNumber);

        if (calculated.ToString() != checkDigit.ToString())
        {
            result.IsValid = false;
            result.Errors.Add("Invalid MRZ checksum.");
            return result;
        }

        result.IsValid = true;
        result.ConfidenceScore = 95;

        return result;
    }

    private int CalculateChecksum(string input)
    {
        int total = 0;

        for (int i = 0; i < input.Length; i++)
        {
            total += GetCharacterValue(input[i]) * weights[i % 3];
        }

        return total % 10;
    }

    private int GetCharacterValue(char c)
    {
        if (char.IsDigit(c))
            return c - '0';

        if (char.IsLetter(c))
            return c - 'A' + 10;

        return 0;
    }
}