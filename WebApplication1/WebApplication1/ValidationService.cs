using DocumentValidator.Application.Interfaces;
using DocumentValidator.Domain.Entities;
using DocumentValidator.Domain.Enums;

namespace DocumentValidator.Application.Services;

public class ValidationService
{
    private readonly IServiceProvider _provider;

    public ValidationService(IServiceProvider provider)
    {
        _provider = provider;
    }

    public DocumentValidationResult Validate(DocumentType type, string value)
    {
        IDocumentValidator validator = type switch
        {
            DocumentType.CPF => _provider.GetRequiredService<CpfValidator>(),
            DocumentType.Passport => _provider.GetRequiredService<PassportMrzValidator>(),
            _ => throw new Exception("Validator not found")
        };

        return validator.Validate(value);
    }
}