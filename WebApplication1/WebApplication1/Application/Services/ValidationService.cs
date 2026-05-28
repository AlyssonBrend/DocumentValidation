using DocumentValidator.Application.Interfaces;
using DocumentValidator.Application.Validators.Brazilian;
using DocumentValidator.Application.Validators.European;
using DocumentValidator.Application.Validators.Passport;
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
            DocumentType.CPF        => _provider.GetRequiredService<CpfValidator>(),
            DocumentType.CNH        => _provider.GetRequiredService<CnhValidator>(),
            DocumentType.Passport   => _provider.GetRequiredService<PassportMrzValidator>(),
            DocumentType.EuropeanId => _provider.GetRequiredService<EuropeanIdValidator>(),
            _ => throw new NotSupportedException($"No validator registered for document type '{type}'.")
        };

        return validator.Validate(value);
    }
}
