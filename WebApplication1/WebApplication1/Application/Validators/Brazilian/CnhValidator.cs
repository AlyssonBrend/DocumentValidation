using System.Text.RegularExpressions;
using DocumentValidator.Application.Interfaces;
using DocumentValidator.Domain.Entities;

namespace DocumentValidator.Application.Validators.Brazilian;

public class CnhValidator : IDocumentValidator
{
    public DocumentValidationResult Validate(string value)
    {
        var result = new DocumentValidationResult();
        var document = Regex.Replace(value, "[^0-9]", "");

        if (document.Length != 11)
        {
            result.Errors.Add("CNH must have 11 digits.");
            return result;
        }

        if (new string(document[0], document.Length) == document)
        {
            result.Errors.Add("Invalid CNH: repeated sequence.");
            return result;
        }

        var numbers = document.Select(c => c - '0').ToArray();

        // First check digit
        int sum1 = 0;
        int factor = 9;
        for (int i = 0; i < 9; i++)
        {
            sum1 += numbers[i] * factor--;
        }
        int firstDigit = sum1 % 11;
        bool carry = firstDigit >= 10;
        firstDigit = carry ? 0 : firstDigit;

        // Second check digit
        int sum2 = 0;
        factor = 1;
        for (int i = 0; i < 9; i++)
        {
            sum2 += numbers[i] * factor++;
        }
        int secondDigit = sum2 % 11;
        if (carry) secondDigit = secondDigit >= 10 ? 0 : secondDigit + 1;
        else secondDigit = secondDigit >= 10 ? 0 : secondDigit;

        if (numbers[9] != firstDigit || numbers[10] != secondDigit)
        {
            result.Errors.Add("Invalid CNH: check digit mismatch.");
            return result;
        }

        result.IsValid = true;
        result.ConfidenceScore = 100;
        return result;
    }
}
