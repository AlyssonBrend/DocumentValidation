using DocumentValidator.Domain.Entities;

namespace DocumentValidator.Application.Interfaces;

public interface IDocumentValidator
{
    DocumentValidationResult Validate(string value);
}
