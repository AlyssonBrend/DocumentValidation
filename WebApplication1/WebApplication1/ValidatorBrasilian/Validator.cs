using System.Text.RegularExpressions;
using DocumentValidator.Application.Interfaces;
using DocumentValidator.Domain.Entities;

namespace DocumentValidator.Application.Validators;

public class CpfValidator : IDocumentValidator
{
    public DocumentValidationResult Validate(string document)
    {
        var result = new DocumentValidationResult();

        // 1. Limpeza rápida com Regex
        document = Regex.Replace(document, "[^0-9]", "");

        // 2. Validação de tamanho
        if (document.Length != 11)
        {
            result.IsValid = false;
            result.Errors.Add("Invalid CPF.");
            return result;
        }

    // 3. Bloqueio de sequências repetidas
        if (new string(document[0], document.Length) == document)
        {
            result.IsValid = false;
            result.Errors.Add("Invalid CPF sequence.");
            return result;
        }

    // 4. Conversão para array de inteiros
        var numbers = document.Select(c => c - '0').ToArray();

    // Funções locais para cálculo dos dígitos verificadores
        int CalculateDigit(int sum)
        {
            int remainder = sum % 11;
            return remainder < 2 ? 0 : 11 - remainder;
        }

    // 5. Validação do Primeiro Dígito
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += numbers[i] * (10 - i);

        if (numbers[9] != CalculateDigit(sum))
        {
            result.IsValid = false;
            result.Errors.Add("Invalid CPF."); 
            return result;
        }

    // 6. Validação do Segundo Dígito
        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += numbers[i] * (11 - i);

        if (numbers[10] != CalculateDigit(sum))
        {
            result.IsValid = false;
            result.Errors.Add("Second verification digit invalid.");
            return result;
        }

    // 7. Sucesso
            result.IsValid = true;
            result.ConfidenceScore = 100;

            return result;
    }
}