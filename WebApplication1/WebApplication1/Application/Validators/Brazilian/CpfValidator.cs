using System.Text.RegularExpressions;
using DocumentValidator.Application.Interfaces;
using DocumentValidator.Domain.Entities;

namespace DocumentValidator.Application.Validators.Brazilian;

public class CpfValidator : IDocumentValidator
{
    public DocumentValidationResult Validate(string value)
    {
        var result = new DocumentValidationResult();
        var document = Regex.Replace(value, "[^0-9]", "");

        if (document.Length != 11)
        {
            result.Errors.Add("CPF must have 11 digits.");
            return result;
        }

        if (new string(document[0], document.Length) == document)
        {
            result.Errors.Add("Invalid CPF: repeated sequence.");
            return result;
        }

        var numbers = document.Select(c => c - '0').ToArray();

        int CalculateDigit(int sum)
        {
            int rem = sum % 11;
            return rem < 2 ? 0 : 11 - rem;
        }

        int sum = 0;
        for (int i = 0; i < 9; i++) sum += numbers[i] * (10 - i);
        if (numbers[9] != CalculateDigit(sum))
        {
            result.Errors.Add("Invalid CPF: first check digit mismatch.");
            return result;
        }

        sum = 0;
        for (int i = 0; i < 10; i++) sum += numbers[i] * (11 - i);
        if (numbers[10] != CalculateDigit(sum))
        {
            result.Errors.Add("Invalid CPF: second check digit mismatch.");
            return result;
        }

        result.IsValid = true;
        result.ConfidenceScore = 100;
        return result;
    }
}
